using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
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

    public struct PathData
    {
        public bool allowPartialSolution;
        public float heightAllowance;
        public bool includeDiagonals;
        public bool includeStartNode;
    }

    public enum PathResult
    {
        SearchFail,
        SearchSuccess,
        GoalUnreachable
    }

    public class GridPathfinding : MonoBehaviour
    {
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

        public event Action<GridIndex> OnPathfindingDataUpdated;
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
        private Dictionary<GridIndex, PathNode> _pathNodePool;

        public PathResult FindPath(GridIndex startIndex, GridIndex targetIndex, out List<GridIndex> outPath, PathData pathData)
        {
            outPath = new List<GridIndex>();

            if (!_tacticsGrid.IsIndexValid(startIndex) || !_tacticsGrid.IsIndexValid(targetIndex))
                return PathResult.SearchFail;
            if (startIndex == targetIndex)
                return PathResult.SearchSuccess;

            _pathNodePool = new Dictionary<GridIndex, PathNode>();
            OnPathfindingDataCleared?.Invoke();
            _frontierNodes = new PriorityQueue<PathNode>();

            //float timeStart = Time.realtimeSinceStartup;

            PathNode startNode = CreateAndAddNodeToPool(startIndex);
            startNode.traversalCost = 0;
            startNode.totalCost = GetHeuristicCost(startIndex, targetIndex) * _heuristicScale;

            _frontierNodes.Enqueue(startNode);
            startNode.isOpened = true;

            PathNode bestNode = startNode;
            float bestNodeCost = startNode.totalCost;
            PathResult pathResult = PathResult.SearchSuccess;

            bool processNodes = true;
            while (_frontierNodes.Count > 0 && processNodes)
            {
                processNodes = ProcessSingleNode(targetIndex, ref bestNode, ref bestNodeCost, pathData);
            }

            if (bestNodeCost != 0f)
                pathResult = PathResult.GoalUnreachable;

            if (pathResult == PathResult.SearchSuccess || pathData.allowPartialSolution)
            {
                outPath = ConvertPathNodesToIndexes(startNode, bestNode);
                //Debug.Log($"PATHFINDER path length = {outPath.Count()}");
            }

            //Debug.Log($"PATHFINDER SearchRoutine: elapsed time = {(Time.realtimeSinceStartup - timeStart) * 1000f}ms");
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

                if (neighbor == currentNode.parent || neighbor == currentNode.index || !IsTraversalAllowed(currentNode.index, neighbor, pathData.heightAllowance))
                    continue;

                PathNode neighborNode = null;
                if (_pathNodePool.TryGetValue(neighbor, out PathNode existingNeighbor))
                    neighborNode = existingNeighbor;
                else
                    neighborNode = CreateAndAddNodeToPool(neighbor);

                if (_ignoreClosed && neighborNode.isClosed)
                    continue;

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
                    OnPathfindingDataUpdated?.Invoke(neighborNode.index);
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

        private List<GridIndex> ConvertPathNodesToIndexes(PathNode startNode, PathNode endNode)
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

            if (!_includeStartNodeInPath)
                tileIndexes.RemoveAt(pathLength - 1);

            tileIndexes.Reverse();

            return tileIndexes;
        }

        private PathNode CreateAndAddNodeToPool(GridIndex index)
        {
            PathNode node = new PathNode();
            node.index = index;
            _pathNodePool.TryAdd(index, node);
            return node;
        }

        public void ClearNodePool()
        {
            _pathNodePool.Clear();
            OnPathfindingDataCleared?.Invoke();
        }

        private float GetTraversalCost(GridIndex source, GridIndex target, float terrainCost)
        {
            float traversalCost = 1f;
            switch (_traversalCost)
            {
                case CalculationType.Chebyshev:
                    traversalCost = Chebyshev(source, target);
                    break;
                case CalculationType.Diagonal:
                    traversalCost = Diagonal(source, target);
                    break;
                case CalculationType.DiagonalShortcut:
                    traversalCost = DiagonalShortcut(source, target);
                    break;
                case CalculationType.Manhattan:
                    traversalCost = Manhattan(source, target);
                    break;
                case CalculationType.Euclidean:
                    traversalCost = Euclidean(source, target);
                    break;
                    
            }
            return traversalCost * terrainCost;
        }

        private float GetHeuristicCost(GridIndex source, GridIndex target)
        {
            switch (_heuristicCost)
            {
                case CalculationType.Chebyshev:
                    return Chebyshev(source, target);
                case CalculationType.Diagonal:
                    return Diagonal(source, target);
                case CalculationType.DiagonalShortcut:
                    return DiagonalShortcut(source, target);
                case CalculationType.Manhattan:
                    return Manhattan(source, target);
                case CalculationType.Euclidean:
                    return Euclidean(source, target);
            }
            return 1f;
        }

        private bool IsTraversalAllowed(GridIndex source, GridIndex target, float heightAllowance)
        {
            // First check if Valid Tile
            if (!GridStatics.IsTileTypeWalkable(_tacticsGrid.GridTiles[target].tileType))
            {
                return false;
            }

            // Height check. We know the tiles are valid because an isValid check is done in the process node function.
            _tacticsGrid.GridTiles.TryGetValue(source, out TileData sourceTile);
            _tacticsGrid.GridTiles.TryGetValue(target, out TileData targetTile);
            float heightDifference = Mathf.Abs(sourceTile.tileMatrix.GetPosition().y - targetTile.tileMatrix.GetPosition().y);
            if (heightDifference > heightAllowance)
            {
                return false;
            }

            //If we don't care about splitting lanes or sharp corners, no need for more math.
            if (_traversalType == TraversalType.AllNonBlocked || _tacticsGrid.GridShape != GridShape.Square) return true;


            //here's where we calculate adjacent tiles to see if our path is being blocked for diagonal movement
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
                        if (heightDifference <= _tacticsGrid.TileSize.y)
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

        //Chess style
        private float Chebyshev(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();
            return Mathf.Max(distance.x, distance.z);
        }

        //Common diagonal allowing square grid heuristic
        public float Diagonal(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();
            float diagonal = Mathf.Sqrt(2);

            return (distance.x + distance.z) + (diagonal - 2) * Mathf.Min(distance.x, distance.z);
        }

        //Same as Diagonal but does 1.4 instead of sqrt(2). Less accurate, more performant.
        public float DiagonalShortcut(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return (distance.x + distance.z) + (1.4f - 2) * Mathf.Min(distance.x, distance.z);
        }

        //Standard algorithm for 4 neighbor squares and Hexagons.
        public float Manhattan(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return distance.x + distance.z;
        }

        //Straight line from start to end calculation
        public float Euclidean(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return Mathf.Sqrt(distance.x * distance.x + distance.z * distance.z);
        }
    }
}