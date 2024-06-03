using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum PathResult
    {
        SearchFail,
        SearchSuccess,
        GoalUnreachable
    }
    public enum CalculationType
    {
        Chebyshev,
        Diagonal,
        DiagonalShortcut,
        Euclidean,
        Manhattan
    }

    public enum TraversalType
    {
        AllNonBlocked,
        NoSharpDiagonals,
        SharpDiagonals
    }
    public struct PathParams
    {
        public bool allowPartialSolution;
        public float heightAllowance;
        public bool includeDiagonals;
        public bool includeStartNode;
        public List<TileType> validTileTypes;
        public float maxPathLength;
    }

    public class PathNode : IComparable<PathNode>
    {
        public GridIndex index = GridIndex.Invalid();
        public float terrainCost = 1f;
        public float traversalCost = Mathf.Infinity;
        public float heuristicCost = Mathf.Infinity;
        public float totalCost = Mathf.Infinity;
        public GridIndex parent = GridIndex.Invalid();

        public bool isOpened;
        public bool isClosed;

        public int CompareTo(PathNode other)
        {
            if (this.totalCost < other.totalCost)
            {
                return -1;
            }
            else if (this.totalCost > other.totalCost)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public struct EdgeData
    {
        public GridIndex source;
        public GridIndex direction;

        public EdgeData(GridIndex index, GridIndex direction)
        {
            this.source = index;
            this.direction = direction;
        }
    }

    public class PathfindingResult
    {
        public PathResult Result { get; set; }
        public List<GridIndex> Path { get; set; }
        public float Length { get; set; }

        public HashSet<EdgeData> Edges { get; set; }
    }

    public class GridPathfinding : MonoBehaviour
    {
        public Action OnPathfindingCompleted;
        public Action OnPathfindingDataUpdated;
        public event Action OnPathfindingDataCleared;

        [SerializeField] private TacticsGrid _tacticsGrid;

        public bool IncludeDiagonals { get => _includeDiagonals; set => _includeDiagonals = value; }
        public float HeightAllowance { get => _heightAllowance; set => _heightAllowance = value; }
        public CalculationType HeuristicCost { get => _heuristicCost; set => _heuristicCost = value; }
        public CalculationType TraversalCost { get => _traversalCost; set => _traversalCost = value; }
        public TraversalType SquareTraversalType { get => _traversalType; set => _traversalType = value; }
        public float HeuristicScale { get => _heuristicScale; set => _heuristicScale = value; }
        public bool AllowPartialSolution { get => _allowPartialSolution; set => _allowPartialSolution = value; }
        public bool IgnoreClosed { get => _ignoreClosed; set => _ignoreClosed = value; }
        public bool IncludeStartNodeInPath { get => _includeStartNodeInPath; set => _includeStartNodeInPath = value; }

        public Dictionary<GridIndex, PathNode> PathNodePool { get => _pathNodePool; }

        private bool _includeDiagonals = false;
        private float _heightAllowance = 2f;
        [Tooltip("Formulas for calculating estimated cost of travel from current node to goal.")]
        [SerializeField] private CalculationType _heuristicCost;
        [Tooltip("Formulas for calculating cost of travel between each node.")]
        [SerializeField] private CalculationType _traversalCost;
        [Tooltip("How do we traverse open nodes near and between walls when moving diagonally. Only applies to 8 direction movement on a square grid")]
        [SerializeField] private TraversalType _traversalType;
        [Tooltip("How much does Heuristics effect solution. 1 is balanced G and H. Lower guarantees shortest path at cost of slow processing(Dijkstra). Higher becomes faster with longer path(Greedy Best First)")]
        private float _heuristicScale = 1f;
        [Tooltip("Allow a returned path that does not reach the end goal")]
        private bool _allowPartialSolution = false;
        [Tooltip("Should we revisit closed nodes for a possibly shorter path at the cost of performance?")]
        private bool _ignoreClosed = false;
        [Tooltip("Should we include the start node in the end result path?")]
        private bool _includeStartNodeInPath = false;

        private PriorityQueue<PathNode> _frontierNodes;
        private PriorityQueue<PathNode> _rangeNodes;
        private Dictionary<GridIndex, PathNode> _pathNodePool = new Dictionary<GridIndex, PathNode>();

        public PathParams CreateDefaultPathParams(float pathLength)
        {
            PathParams pathParams;
            pathParams.heightAllowance = _heightAllowance;
            pathParams.includeDiagonals = _includeDiagonals;
            pathParams.allowPartialSolution = _allowPartialSolution;
            pathParams.includeStartNode = _includeStartNodeInPath;
            pathParams.validTileTypes = new List<TileType> { TileType.Normal, TileType.DoubleCost, TileType.TripleCost };
            pathParams.maxPathLength = pathLength;

            return pathParams;
        }

        public static PathParams CreatePathParamsFromUnit(Unit unit, float pathLength = 1000f, bool allowPartialSolution = false, bool includeStartNode = false)
        {
            PathParams pathParams;
            pathParams.includeDiagonals = unit.UnitData.unitStats.canMoveDiagonal;
            pathParams.heightAllowance = unit.UnitData.unitStats.heightAllowance;
            pathParams.includeStartNode = includeStartNode;
            pathParams.allowPartialSolution = allowPartialSolution;
            pathParams.validTileTypes = unit.UnitData.unitStats.validTileTypes;
            pathParams.maxPathLength = pathLength;

            return pathParams;
        }

        public PathfindingResult FindTilesInRange(GridIndex startIndex, PathParams pathParams)
        {
            PathfindingResult pathResult = new PathfindingResult();
            pathResult.Path = new List<GridIndex>();
            pathResult.Result = PathResult.SearchSuccess;
            pathResult.Edges = new HashSet<EdgeData>();

            if (!_tacticsGrid.IsIndexValid(startIndex))
            {
                pathResult.Result = PathResult.SearchFail;
                return pathResult;
            }

            _pathNodePool.Clear();
            _rangeNodes = new PriorityQueue<PathNode>();
            List<GridIndex> indexesInRange = new List<GridIndex>();

            if (pathParams.includeStartNode)
                indexesInRange.Add(startIndex);

            PathNode startNode = CreateAndAddNodeToPool(startIndex);
            startNode.traversalCost = 0;
            startNode.totalCost = 0;
            _rangeNodes.Enqueue(startNode);

            while (_rangeNodes.Count > 0)
            {
                PathNode currentNode = _rangeNodes.Dequeue();

                for (int i = 0; i < GetNeighborCount(pathParams.includeDiagonals); i++)
                {
                    GridIndex neighborIndex = GetNeighborIndexFromArray(currentNode.index, i);

                    if (!_tacticsGrid.IsIndexValid(neighborIndex))
                    {
                        if (i < GetNeighborCount(false))
                            pathResult.Edges.Add(new EdgeData(currentNode.index, GridStatics.SquareNeighbors[i]));
                        continue;
                    }

                    if (neighborIndex == currentNode.parent)
                        continue;

                    if (!IsTraversalAllowed(currentNode.index, neighborIndex, pathParams.heightAllowance, pathParams.validTileTypes))
                    {
                        if (i < GetNeighborCount(false))
                            pathResult.Edges.Add(new EdgeData(currentNode.index, GridStatics.SquareNeighbors[i]));
                        continue;
                    }

                    PathNode neighborNode = null;
                    if (_pathNodePool.TryGetValue(neighborIndex, out PathNode existingNeighbor))
                    {
                        neighborNode = existingNeighbor;
                    }
                    else
                        neighborNode = CreateAndAddNodeToPool(neighborIndex);

                    neighborNode.terrainCost = PathfindingStatics.GetTerrainCostFromTileType(_tacticsGrid.GridTiles[neighborNode.index].tileType);

                    float newTraversalCost = currentNode.traversalCost + (GetTraversalCost(currentNode.index, neighborNode.index) * neighborNode.terrainCost);

                    if (newTraversalCost >= neighborNode.traversalCost)
                        continue;

                    if (newTraversalCost > pathParams.maxPathLength)
                    {
                        if (i < GetNeighborCount(false))
                        {
                            pathResult.Edges.Add(new EdgeData(currentNode.index, GridStatics.SquareNeighbors[i]));
                        }
                        continue;
                    }

                    neighborNode.traversalCost = newTraversalCost;
                    neighborNode.totalCost = newTraversalCost; //Our GridIndexes do a comparison on totalCost for A* and that's what our PriorityQueue checks. We set the totalCost so it functions as desired in the queue.
                    neighborNode.parent = currentNode.index;

                    if (!neighborNode.isOpened)
                    {
                        neighborNode.isOpened = true;
                        _rangeNodes.Enqueue(neighborNode);
                        indexesInRange.Add(neighborNode.index);
                    }
                }
            }
            pathResult.Path = indexesInRange;
            return pathResult;
        }

        public PathfindingResult FindPath(GridIndex startIndex, GridIndex targetIndex, PathParams pathParams)
        {
            PathfindingResult pathResult = new PathfindingResult();
            pathResult.Path = new List<GridIndex>();
            pathResult.Result = PathResult.SearchSuccess;

            if (!_tacticsGrid.IsIndexValid(startIndex) || !_tacticsGrid.IsIndexValid(targetIndex))
            {
                pathResult.Result = PathResult.SearchFail;
                return pathResult;
            }
            if (startIndex == targetIndex)
            {
                pathResult.Result = PathResult.SearchSuccess;
                return pathResult; ;
            }

            _pathNodePool.Clear();
            _frontierNodes = new PriorityQueue<PathNode>();

            PathNode startNode = CreateAndAddNodeToPool(startIndex);
            startNode.traversalCost = 0;
            startNode.totalCost = GetHeuristicCost(startIndex, targetIndex) * _heuristicScale;

            _frontierNodes.Enqueue(startNode);
            startNode.isOpened = true;

            PathNode bestNode = startNode;
            float bestNodeCost = startNode.totalCost;

            bool processNodes = true;
            while (_frontierNodes.Count > 0 && processNodes)
            {
                processNodes = ProcessSingleNode(targetIndex, ref bestNode, ref bestNodeCost, pathParams);
            }

            if (bestNodeCost != 0f)
                pathResult.Result = PathResult.GoalUnreachable;

            if (pathResult.Result == PathResult.SearchSuccess || pathParams.allowPartialSolution)
            {
                pathResult.Path = ConvertPathNodesToIndexes(startNode, bestNode, pathParams.includeStartNode);
                pathResult.Length = bestNode.totalCost;
            }
            return pathResult;
        }

        private bool ProcessSingleNode(GridIndex goalNode, ref PathNode bestNode, ref float bestNodeCost, PathParams pathParams)
        {
            PathNode currentNode = _frontierNodes.Dequeue();
            currentNode.isClosed = true;

            if (currentNode.index == goalNode)
            {
                bestNode = currentNode;
                bestNodeCost = 0f;
                return false;
            }

            for (int i = 0; i < GetNeighborCount(pathParams.includeDiagonals); i++)
            {
                GridIndex neighborIndex = GetNeighborIndexFromArray(currentNode.index, i);

                if (!_tacticsGrid.IsIndexValid(neighborIndex))
                    continue;

                if (neighborIndex == currentNode.parent || !IsTraversalAllowed(currentNode.index, neighborIndex, pathParams.heightAllowance, pathParams.validTileTypes))
                    continue;

                PathNode neighborNode = null;
                if (_pathNodePool.TryGetValue(neighborIndex, out PathNode existingNeighbor))
                    neighborNode = existingNeighbor;
                else
                    neighborNode = CreateAndAddNodeToPool(neighborIndex);

                if (_ignoreClosed && neighborNode.isClosed)
                    continue;

                neighborNode.terrainCost = PathfindingStatics.GetTerrainCostFromTileType(_tacticsGrid.GridTiles[neighborNode.index].tileType);

                float newTraversalCost = currentNode.traversalCost + (GetTraversalCost(currentNode.index, neighborNode.index) * neighborNode.terrainCost);

                if (newTraversalCost > pathParams.maxPathLength)
                    continue;

                float newHeuristic = GetHeuristicCost(neighborNode.index, goalNode) * _heuristicScale;
                float newTotalCost = newTraversalCost + newHeuristic;

                if (newTotalCost >= neighborNode.totalCost)
                    continue;

                neighborNode.traversalCost = newTraversalCost;
                neighborNode.heuristicCost = newHeuristic;
                neighborNode.totalCost = newTotalCost;
                neighborNode.parent = currentNode.index;
                neighborNode.isClosed = false;

                if (!neighborNode.isOpened)
                {
                    _frontierNodes.Enqueue(neighborNode);
                    neighborNode.isOpened = true;
                }

                if (newHeuristic < bestNodeCost)
                {
                    bestNodeCost = newHeuristic;
                    bestNode = neighborNode;
                }
            }
            return true;
        }

        private List<GridIndex> ConvertPathNodesToIndexes(PathNode startNode, PathNode endNode, bool includeStartNode)
        {
            List<GridIndex> tileIndexes = new List<GridIndex>();

            tileIndexes.Add(endNode.index);

            PathNode currentNode = endNode;

            int pathLength = tileIndexes.Count;
            while (currentNode.index != startNode.index)
            {
                tileIndexes.Add(currentNode.parent);
                currentNode = _pathNodePool[currentNode.parent];
                pathLength++;
            }

            if (!includeStartNode)
                tileIndexes.RemoveAt(pathLength - 1);

            tileIndexes.Reverse();

            return tileIndexes;
        }

        public PathNode CreateAndAddNodeToPool(GridIndex index)
        {
            if (_pathNodePool == null)
                _pathNodePool = new Dictionary<GridIndex, PathNode>();

            PathNode node = new PathNode();
            node.index = index;
            _pathNodePool.TryAdd(index, node);
            return node;
        }

        public void ClearNodePool()
        {
            if (_pathNodePool == null)
                return;

            _pathNodePool.Clear();
            OnPathfindingDataCleared?.Invoke();
        }

        private float GetTraversalCost(GridIndex source, GridIndex target)
        {
            if (_tacticsGrid.GridShape == GridShape.Hexagon)
            {
                return PathfindingStatics.GetDistanceFromAxialCoordinates(PathfindingStatics.ConvertOddrToAxial(source), PathfindingStatics.ConvertOddrToAxial(target));
            }
            if (_tacticsGrid.GridShape == GridShape.Triangle)
            {
                return PathfindingStatics.GetTriangleDistance(source, target);
            }

            float traversalCost = 1f;
            switch (_traversalCost)
            {
                case CalculationType.Chebyshev:
                    traversalCost = PathfindingStatics.GetChebyshevDistance(source, target);
                    break;
                case CalculationType.Diagonal:
                    traversalCost = PathfindingStatics.GetDiagonalDistance(source, target);
                    break;
                case CalculationType.DiagonalShortcut:
                    traversalCost = PathfindingStatics.GetDiagonalShortcutDistance(source, target);
                    break;
                case CalculationType.Manhattan:
                    traversalCost = PathfindingStatics.GetManhattanDistance(source, target);
                    break;
                case CalculationType.Euclidean:
                    traversalCost = PathfindingStatics.GetEuclideanDistance(source, target);
                    break;

            }
            return traversalCost;
        }

        public float GetHeuristicCost(GridIndex source, GridIndex target)
        {
            if (_tacticsGrid.GridShape == GridShape.Hexagon)
            {
                return PathfindingStatics.GetDistanceFromAxialCoordinates(PathfindingStatics.ConvertOddrToAxial(source), PathfindingStatics.ConvertOddrToAxial(target));
            }
            if (_tacticsGrid.GridShape == GridShape.Triangle)
            {
                return PathfindingStatics.GetTriangleDistance(source, target);
            }

            switch (_heuristicCost)
            {
                case CalculationType.Chebyshev:
                    return PathfindingStatics.GetChebyshevDistance(source, target);
                case CalculationType.Diagonal:
                    return PathfindingStatics.GetDiagonalDistance(source, target);
                case CalculationType.DiagonalShortcut:
                    return PathfindingStatics.GetDiagonalShortcutDistance(source, target);
                case CalculationType.Manhattan:
                    return PathfindingStatics.GetManhattanDistance(source, target);
                case CalculationType.Euclidean:
                    return PathfindingStatics.GetEuclideanDistance(source, target);
            }
            return 1f;
        }

        private bool IsValidTileType(List<TileType> validTypes, TileType typeBeingChecked)
        {
            for (int i = 0; i < validTypes.Count; i++)
            {
                if (typeBeingChecked == validTypes[i])
                    return true;
            }
            return false;
        }

        private bool IsTraversalAllowed(GridIndex source, GridIndex target, float heightAllowance, List<TileType> validTileTypes)
        {

            // First check if Valid Tile
            if (!IsValidTileType(validTileTypes, _tacticsGrid.GridTiles[target].tileType))
            {
                return false;
            }

            _tacticsGrid.GridTiles.TryGetValue(target, out TileData targetTile);
            if (targetTile.unitOnTile)
                return false;

            // Height check. We know the tiles are valid because an isValid check is done in the process node function.
            _tacticsGrid.GridTiles.TryGetValue(source, out TileData sourceTile);
            float heightDifference = Mathf.Abs(sourceTile.tileMatrix.GetPosition().y - targetTile.tileMatrix.GetPosition().y);
            if (heightDifference > heightAllowance)
            {
                if (sourceTile.climbData.hasClimbLink)
                {
                    if (sourceTile.climbData.climbLinks.Contains(targetTile.index))
                        return true;
                }
                return false;
            }

            //If we don't care about splitting lanes or sharp corners, no need for more math.
            if (_traversalType == TraversalType.AllNonBlocked || _tacticsGrid.GridShape != GridShape.Square) return true;


            //here's where we evaluate adjacent tiles to see if our path is being blocked for diagonal movement
            int srcX = source.x;
            int srcZ = source.z;
            GridIndex distance = target - source;

            if (Mathf.Abs(distance.x * distance.z) == 1)
            {
                //Make sure these adjacent spaces are valid
                bool isValidOne = _tacticsGrid.GridTiles.TryGetValue(new GridIndex(srcX + distance.x, srcZ), out TileData adjacentTileOne);
                bool isValidTwo = _tacticsGrid.GridTiles.TryGetValue(new GridIndex(srcX, srcZ + distance.z), out TileData adjacentTileTwo);

                if (_traversalType == TraversalType.NoSharpDiagonals)
                {
                    if (!isValidOne || !isValidTwo) return false;

                    if ((!GridStatics.IsTileTypeWalkable(adjacentTileOne.tileType))
                     || (!GridStatics.IsTileTypeWalkable(adjacentTileTwo.tileType)))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!isValidOne && !isValidTwo) return false;

                    if ((isValidOne && !GridStatics.IsTileTypeWalkable(adjacentTileOne.tileType))
                     && (isValidTwo && !GridStatics.IsTileTypeWalkable(adjacentTileTwo.tileType)))
                    {
                        return false;
                    }

                    //If there's a wall, it's likely the tile on top is valid. We do a height check and if there's a valid tile above on one of the adjacent tiles, that means a wall is there. Treat the tile like an obstacle if it's above our allowance.
                    float heightCheckOne = adjacentTileOne.tileMatrix.GetPosition().y - sourceTile.tileMatrix.GetPosition().y;
                    float heightCheckTwo = adjacentTileTwo.tileMatrix.GetPosition().y - sourceTile.tileMatrix.GetPosition().y;
                    if (heightCheckOne > heightAllowance || heightCheckTwo > heightAllowance)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private int GetNeighborCount(bool includeDiagonals)
        {
            switch (_tacticsGrid.GridShape)
            {
                case GridShape.Square:
                    return includeDiagonals ? 8 : 4;
                case GridShape.Hexagon:
                    return 6;
                case GridShape.Triangle:
                    return includeDiagonals ? 6 : 3;
            }
            return 0;
        }

        public List<GridIndex> GetTileNeighbors(GridIndex index, bool includeDiagonals = false)
        {
            List<GridIndex> neighbors = new List<GridIndex>();
            for (int i = 0; i < GetNeighborCount(includeDiagonals); i++)
            {
                neighbors.Add(GetNeighborIndexFromArray(index, i));
            }
            return neighbors;
        }

        public GridIndex GetNeighborIndexFromArray(GridIndex gridIndex, int arrayIndex)
        {
            switch (_tacticsGrid.GridShape)
            {
                case GridShape.Square:
                    return GridStatics.GetSquareNeighborAtIndex(gridIndex, arrayIndex);
                case GridShape.Hexagon:
                    return GridStatics.GetHexagonNeighborAtIndex(gridIndex, arrayIndex);
                case GridShape.Triangle:
                    return GridStatics.GetTriangleNeighborAtIndex(gridIndex, arrayIndex);
            }
            return GridIndex.Invalid();
        }
    }
}