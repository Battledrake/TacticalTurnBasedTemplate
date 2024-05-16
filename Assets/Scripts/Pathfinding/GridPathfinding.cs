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
    public struct PathFilter
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
        public GridIndex index = new GridIndex(int.MinValue, int.MinValue);
        public float terrainCost = 1f;
        public float traversalCost = Mathf.Infinity;
        public float heuristicCost = Mathf.Infinity;
        public float totalCost = Mathf.Infinity;
        public GridIndex parent = new GridIndex(int.MinValue, int.MinValue);

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

    public class PathfindingResult
    {
        public PathResult Result { get; set; }
        public List<GridIndex> Path { get; set; }
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
        private bool _ignoreClosed = true;
        [Tooltip("Should we include the start node in the end result path?")]
        private bool _includeStartNodeInPath = false;

        private PriorityQueue<PathNode> _frontierNodes;
        private Queue<PathNode> _rangeNodes;
        private Dictionary<GridIndex, PathNode> _pathNodePool = new Dictionary<GridIndex, PathNode>();

        public PathFilter CreateDefaultPathFilter(float pathLength)
        {
            PathFilter pathFilter;
            pathFilter.heightAllowance = _heightAllowance;
            pathFilter.includeDiagonals = _includeDiagonals;
            pathFilter.allowPartialSolution = _allowPartialSolution;
            pathFilter.includeStartNode = _includeStartNodeInPath;
            pathFilter.validTileTypes = new List<TileType> { TileType.Normal, TileType.DoubleCost, TileType.TripleCost };
            pathFilter.maxPathLength = pathLength;

            return pathFilter;
        }

        public static PathFilter CreatePathFilterFromUnit(Unit unit, bool allowPartialSolution = false, bool includeStartNode = false)
        {
            PathFilter pathFilter;
            pathFilter.includeDiagonals = unit.UnitData.unitStats.canMoveDiagonal;
            pathFilter.heightAllowance = unit.UnitData.unitStats.heightAllowance;
            pathFilter.includeStartNode = includeStartNode;
            pathFilter.allowPartialSolution = allowPartialSolution;
            pathFilter.validTileTypes = unit.UnitData.unitStats.validTileTypes;
            pathFilter.maxPathLength = unit.UnitData.unitStats.moveRange;

            return pathFilter;
        }

        public PathfindingResult FindTilesInRange(GridIndex startIndex, PathFilter pathFilter)
        {
            PathfindingResult pathResult = new PathfindingResult();
            pathResult.Path = new List<GridIndex>();
            pathResult.Result = PathResult.SearchSuccess;

            if (!_tacticsGrid.IsIndexValid(startIndex))
            {
                pathResult.Result = PathResult.SearchFail;
                return pathResult;
            }

            _pathNodePool.Clear();
            _rangeNodes = new Queue<PathNode>();
            List<GridIndex> indexesInRange = new List<GridIndex>();

            PathNode startNode = CreateAndAddNodeToPool(startIndex);
            startNode.traversalCost = 0;
            _rangeNodes.Enqueue(startNode);

            while (_rangeNodes.Count > 0)
            {
                PathNode currentNode = _rangeNodes.Dequeue();
                if (!indexesInRange.Contains(currentNode.index))
                    indexesInRange.Add(currentNode.index);


                for (int i = 0; i < GetNeighborCount(pathFilter.includeDiagonals); i++)
                {
                    GridIndex neighborIndex = GetNeighborIndexFromArray(currentNode.index, i);

                    if (!_tacticsGrid.IsIndexValid(neighborIndex))
                        continue;

                    if (neighborIndex == currentNode.parent || neighborIndex == currentNode.index)
                        continue;

                    if (!IsTraversalAllowed(currentNode.index, neighborIndex, pathFilter.heightAllowance, pathFilter.validTileTypes))
                        continue;

                    PathNode neighborNode = null;
                    if (_pathNodePool.TryGetValue(neighborIndex, out PathNode existingNeighbor))
                        neighborNode = existingNeighbor;
                    else
                        neighborNode = CreateAndAddNodeToPool(neighborIndex);

                    neighborNode.terrainCost = GridStatics.GetTerrainCostFromTileType(_tacticsGrid.GridTiles[neighborNode.index].tileType);

                    float newTraversalCost = currentNode.traversalCost + (GetTraversalCost(currentNode.index, neighborNode.index) * neighborNode.terrainCost);

                    if (newTraversalCost > pathFilter.maxPathLength)
                        continue;

                    if (newTraversalCost > neighborNode.traversalCost)
                        continue;

                    neighborNode.traversalCost = newTraversalCost;
                    neighborNode.parent = currentNode.index;

                    _rangeNodes.Enqueue(neighborNode);
                    indexesInRange.Add(currentNode.index);
                }
            }
            pathResult.Path = indexesInRange;
            return pathResult;
        }

        public PathfindingResult FindPath(GridIndex startIndex, GridIndex targetIndex, PathFilter pathFilter)
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
                processNodes = ProcessSingleNode(targetIndex, ref bestNode, ref bestNodeCost, pathFilter);
            }

            if (bestNodeCost != 0f)
                pathResult.Result = PathResult.GoalUnreachable;

            if (pathResult.Result == PathResult.SearchSuccess || pathFilter.allowPartialSolution)
            {
                pathResult.Path = ConvertPathNodesToIndexes(startNode, bestNode, pathFilter.includeStartNode);
            }
            return pathResult;
        }

        private bool ProcessSingleNode(GridIndex goalNode, ref PathNode bestNode, ref float bestNodeCost, PathFilter pathFilter)
        {
            PathNode currentNode = _frontierNodes.Dequeue();
            currentNode.isClosed = true;

            if (currentNode.index == goalNode)
            {
                bestNode = currentNode;
                bestNodeCost = 0f;
                return false;
            }

            for (int i = 0; i < GetNeighborCount(pathFilter.includeDiagonals); i++)
            {
                GridIndex neighborIndex = GetNeighborIndexFromArray(currentNode.index, i);

                if (!_tacticsGrid.IsIndexValid(neighborIndex))
                    continue;

                if (neighborIndex == currentNode.parent || neighborIndex == currentNode.index || !IsTraversalAllowed(currentNode.index, neighborIndex, pathFilter.heightAllowance, pathFilter.validTileTypes))
                    continue;

                PathNode neighborNode = null;
                if (_pathNodePool.TryGetValue(neighborIndex, out PathNode existingNeighbor))
                    neighborNode = existingNeighbor;
                else
                    neighborNode = CreateAndAddNodeToPool(neighborIndex);

                if (_ignoreClosed && neighborNode.isClosed)
                    continue;

                neighborNode.terrainCost = GridStatics.GetTerrainCostFromTileType(_tacticsGrid.GridTiles[neighborNode.index].tileType);

                float newTraversalCost = currentNode.traversalCost + (GetTraversalCost(currentNode.index, neighborNode.index) * neighborNode.terrainCost);

                if (newTraversalCost > pathFilter.maxPathLength)
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
                return GetDistanceFromAxialCoordinates(ConvertOddrToAxial(source), ConvertOddrToAxial(target));
            }
            if (_tacticsGrid.GridShape == GridShape.Triangle)
            {
                return GetTriangleDistance(source, target);
            }

            float traversalCost = 1f;
            switch (_traversalCost)
            {
                case CalculationType.Chebyshev:
                    traversalCost = GetChebyshevDistance(source, target);
                    break;
                case CalculationType.Diagonal:
                    traversalCost = GetDiagonalDistance(source, target);
                    break;
                case CalculationType.DiagonalShortcut:
                    traversalCost = GetDiagonalShortcutDistance(source, target);
                    break;
                case CalculationType.Manhattan:
                    traversalCost = GetManhattanDistance(source, target);
                    break;
                case CalculationType.Euclidean:
                    traversalCost = GetEuclideanDistance(source, target);
                    break;

            }
            return traversalCost;
        }

        public float GetHeuristicCost(GridIndex source, GridIndex target)
        {
            if (_tacticsGrid.GridShape == GridShape.Hexagon)
            {
                return GetDistanceFromAxialCoordinates(ConvertOddrToAxial(source), ConvertOddrToAxial(target));
            }
            if (_tacticsGrid.GridShape == GridShape.Triangle)
            {
                return GetTriangleDistance(source, target);
            }

            switch (_heuristicCost)
            {
                case CalculationType.Chebyshev:
                    return GetChebyshevDistance(source, target);
                case CalculationType.Diagonal:
                    return GetDiagonalDistance(source, target);
                case CalculationType.DiagonalShortcut:
                    return GetDiagonalShortcutDistance(source, target);
                case CalculationType.Manhattan:
                    return GetManhattanDistance(source, target);
                case CalculationType.Euclidean:
                    return GetEuclideanDistance(source, target);
            }
            return 1f;
        }

        private bool IsValidTileType(List<TileType> validTypes, TileType typeBeingChecked)
        {
            for (int i = 0; i < validTypes.Count; i++)
            {
                if (typeBeingChecked == validTypes.ElementAt(i))
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
                    return GridStatics.GetSquareNeighborIndex(gridIndex, arrayIndex);
                case GridShape.Hexagon:
                    return GridStatics.GetHexagonNeighborIndex(gridIndex, arrayIndex);
                case GridShape.Triangle:
                    return GridStatics.GetTriangleNeighborIndex(gridIndex, arrayIndex);
            }
            return GridIndex.Invalid();
        }

        public GridIndex ConvertOddrToAxial(GridIndex hex)
        {
            var q = hex.x - (hex.z - (hex.z & 1)) / 2;
            var r = hex.z;
            return new GridIndex(q, r);
        }

        public float GetDistanceFromAxialCoordinates(GridIndex source, GridIndex target)
        {
            int dq = Mathf.Abs(source.x - target.x);
            int dr = Mathf.Abs(source.z - target.z);
            int ds = Mathf.Abs((source.x + source.z) - (target.x + target.z));

            return Mathf.Max(dq, dr, ds);
        }

        public float GetTriangleDistance(GridIndex source, GridIndex target)
        {
            float triangleHeight = Mathf.Sqrt(3) / 2;
            float triangleWidth = _tacticsGrid.TileSize.x;

            float x1 = source.x + (source.z & 1) * 0.5f;
            float y1 = source.z * triangleHeight;
            float x2 = target.x + (target.z & 1) * 0.5f;
            float y2 = target.z * triangleHeight;

            return Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
        }

        //Chess style
        private float GetChebyshevDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();
            return Mathf.Max(distance.x, distance.z);
        }

        //Common diagonal allowing square grid heuristic
        public float GetDiagonalDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();
            float diagonal = Mathf.Sqrt(2);

            return (distance.x + distance.z) + (diagonal - 2) * Mathf.Min(distance.x, distance.z);
        }

        //Same as Diagonal but does 1.4 instead of sqrt(2). Less accurate, more performant.
        public float GetDiagonalShortcutDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return (distance.x + distance.z) + (1.4f - 2) * Mathf.Min(distance.x, distance.z);
        }

        //Standard algorithm for 4 neighbor squares and Hexagons.
        public float GetManhattanDistance(GridIndex source, GridIndex target)
        {
            if (_tacticsGrid.GridShape == GridShape.Hexagon)
            {
                GridIndex sourceAxial = ConvertOddrToAxial(source);
                GridIndex targetAxial = ConvertOddrToAxial(target);

                int x1 = sourceAxial.x;
                int z1 = sourceAxial.z;
                int y1 = -x1 - z1;

                int x2 = targetAxial.x;
                int z2 = targetAxial.z;
                int y2 = -x2 - z2;

                return (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) + Mathf.Abs(z1 - z2)) / 2;
            }
            GridIndex distance = (source - target).Abs();

            return distance.x + distance.z;
        }

        //Straight line from start to end calculation
        public float GetEuclideanDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return Mathf.Sqrt(distance.x * distance.x + distance.z * distance.z);
        }
    }
}