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
    public struct PathData
    {
        public bool allowPartialSolution;
        public float heightAllowance;
        public bool includeDiagonals;
        public bool includeStartNode;
        public List<TileType> validTileTypes;
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
        public Action<GridIndex> OnPathfindingDataUpdated;
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
        [Tooltip("Should we revisit closed nodes for a possibly short path at the cost of performance?")]
        private bool _ignoreClosed = true;
        [Tooltip("Should we include the start node in the end result path?")]
        private bool _includeStartNodeInPath = true;

        private PriorityQueue<PathNode> _frontierNodes;
        private Dictionary<GridIndex, PathNode> _pathNodePool = new Dictionary<GridIndex, PathNode>();

        //This exists so that there can be Units that find a path with custom data options. Otherwise, use default values.
        public PathfindingResult FindPath(GridIndex startIndex, GridIndex targetIndex)
        {
            PathData pathData;
            pathData.heightAllowance = _heightAllowance;
            pathData.includeDiagonals = _includeDiagonals;
            pathData.allowPartialSolution = _allowPartialSolution;
            pathData.includeStartNode = _includeStartNodeInPath;
            pathData.validTileTypes = new List<TileType>{ TileType.Normal, TileType.DoubleCost, TileType.TripleCost};

            return FindPath(startIndex, targetIndex, pathData);
        }

        public PathfindingResult FindPath(GridIndex startIndex, GridIndex targetIndex, PathData pathData)
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
            pathResult.Result = PathResult.SearchSuccess;

            bool processNodes = true;
            while (_frontierNodes.Count > 0 && processNodes)
            {
                processNodes = ProcessSingleNode(targetIndex, ref bestNode, ref bestNodeCost, pathData);
            }

            if (bestNodeCost != 0f)
                pathResult.Result = PathResult.GoalUnreachable;

            if (pathResult.Result == PathResult.SearchSuccess || pathData.allowPartialSolution)
            {
                pathResult.Path = ConvertPathNodesToIndexes(startNode, bestNode, pathData.includeStartNode);
            }
            return pathResult;
        }

        private bool ProcessSingleNode(GridIndex goalNode, ref PathNode bestNode, ref float bestNodeCost, PathData pathData)
        {
            PathNode currentNode = _frontierNodes.Dequeue();
            currentNode.isClosed = true;

            if (currentNode.index == goalNode)
            {
                bestNode = currentNode;
                bestNodeCost = 0f;
                return false;
            }

            for (int i = 0; i < GetNeighborCount(pathData.includeDiagonals); i++)
            {
                GridIndex neighbor = GetNeighborIndex(currentNode.index, i, pathData.includeDiagonals);

                if (!_tacticsGrid.IsIndexValid(neighbor))
                    continue;

                if (neighbor == currentNode.parent || neighbor == currentNode.index || !IsTraversalAllowed(currentNode.index, neighbor, pathData.heightAllowance, pathData.validTileTypes))
                    continue;

                PathNode neighborNode = null;
                if (_pathNodePool.TryGetValue(neighbor, out PathNode existingNeighbor))
                    neighborNode = existingNeighbor;
                else
                    neighborNode = CreateAndAddNodeToPool(neighbor);

                if (_ignoreClosed && neighborNode.isClosed)
                    continue;

                neighborNode.terrainCost = (int)_tacticsGrid.GridTiles[neighborNode.index].tileType; //HACK: only valid tiles will be normal/double/triple at indexes 1,2, 3 of the enum. Implement custom method later.

                float newTraversalCost = GetTraversalCost(currentNode.index, neighborNode.index, neighborNode.terrainCost) + currentNode.traversalCost;
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

        private float GetTraversalCost(GridIndex source, GridIndex target, float terrainCost)
        {
            if (_tacticsGrid.GridShape == GridShape.Hexagon)
            {
                return GetDistanceFromAxialCoordinates(ConvertOddrToAxial(source), ConvertOddrToAxial(target)) * terrainCost;
            }
            if (_tacticsGrid.GridShape == GridShape.Triangle)
            {
                return GetTriangleDistance(source, target) * terrainCost;
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
            return traversalCost * terrainCost;
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
            for(int i = 0; i < validTypes.Count; i++)
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

        public List<GridIndex> GetValidTileNeighbors(GridIndex index, bool includeDiagonals = false)
        {
            List<GridIndex> neighborIndexes = GetNeighborIndexes(index, includeDiagonals);
            List<GridIndex> validNeighbors = new List<GridIndex>();
            _tacticsGrid.GridTiles.TryGetValue(index, out TileData selectedTile);

            for (int i = 0; i < neighborIndexes.Count; i++)
            {
                if (_tacticsGrid.GridTiles.TryGetValue(neighborIndexes[i], out TileData tileData))
                {
                    if (GridStatics.IsTileTypeWalkable(tileData.tileType))
                    {
                        float heightDifference = Mathf.Abs(tileData.tileMatrix.GetPosition().y - selectedTile.tileMatrix.GetPosition().y);
                        if (heightDifference <= _heightAllowance)
                        {
                            validNeighbors.Add(tileData.index);
                        }
                    }
                }
            }
            return validNeighbors;
        }

        public GridIndex GetNeighborIndex(GridIndex index, int neighborIndex, bool includeDiagonals = false)
        {
            if (GetNeighborIndexes(index).Count > 0)
                return GetNeighborIndexes(index, includeDiagonals)[neighborIndex];
            else
                return new GridIndex(-999, -999);
        }

        public List<GridIndex> GetNeighborIndexes(GridIndex index, bool includeDiagonals = false)
        {
            switch (_tacticsGrid.GridShape)
            {
                case GridShape.Square:
                    return GetNeighborIndexesForSquare(index, includeDiagonals);
                case GridShape.Hexagon:
                    return GetNeighborIndexesForHexagon(index);
                case GridShape.Triangle:
                    return GetNeighborIndexesForTriangle(index, includeDiagonals);
            }
            return new List<GridIndex>();
        }

        private List<GridIndex> GetNeighborIndexesForSquare(GridIndex index, bool includeDiagonals = false)
        {
            List<GridIndex> neighbors = new List<GridIndex>
            {
                index + new GridIndex(1, 0),
                index + new GridIndex(0, 1),
                index + new GridIndex(-1, 0),
                index + new GridIndex(0, -1)
            };

            if (includeDiagonals)
            {
                neighbors.Add(index + new GridIndex(1, 1));
                neighbors.Add(index + new GridIndex(-1, 1));
                neighbors.Add(index + new GridIndex(-1, -1));
                neighbors.Add(index + new GridIndex(1, -1));
            }
            return neighbors;
        }

        private List<GridIndex> GetNeighborIndexesForHexagon(GridIndex index)
        {
            bool isOddRow = index.z % 2 == 1;
            List<GridIndex> neighbors = new List<GridIndex>
            {
                index + new GridIndex(-1, 0),
                index + new GridIndex(1, 0),
                index + new GridIndex(isOddRow ? 1 : -1, 1),
                index + new GridIndex(0, 1),
                index + new GridIndex(isOddRow ? 1 : -1, -1),
                index + new GridIndex(0, -1)
            };
            return neighbors;
        }

        private List<GridIndex> GetNeighborIndexesForTriangle(GridIndex index, bool includeDiagonals = false)
        {
            bool isFacingUp = index.x % 2 == index.z % 2;
            List<GridIndex> neighbors = new List<GridIndex>
            {
                index + new GridIndex(-1, 0),
                index + new GridIndex(0, isFacingUp ? -1 : 1),
                index + new GridIndex(1, 0)
            };

            if (includeDiagonals)
            {
                neighbors.Add(index + new GridIndex(-2, isFacingUp ? -1 : 1));
                neighbors.Add(index + new GridIndex(0, isFacingUp ? 1 : -1));
                neighbors.Add(index + new GridIndex(2, isFacingUp ? -1 : 1));
            }
            return neighbors;
        }

        public GridIndex ConvertOddrToAxial(GridIndex hex)
        {
            var q = hex.x - (hex.z - (hex.z & 1)) / 2;
            var r = hex.z;
            return new GridIndex(q, r);
        }

        public float GetDistanceFromAxialCoordinates(GridIndex source, GridIndex target)
        {
            GridIndex distance = source - target;
            return (Mathf.Abs(distance.x) + Mathf.Abs(distance.x + distance.z) + Mathf.Abs(distance.z)) / 2;
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