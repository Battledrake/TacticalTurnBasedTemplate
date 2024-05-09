using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class PathNode : IComparable<PathNode>
    {
        public GridIndex index = new GridIndex(-999, -999);
        public int terrainCost = 1;
        public float traversalCost = int.MaxValue;
        public float totalCost = int.MaxValue;
        public GridIndex parent = new GridIndex(-999, -999);

        public bool isOpened;
        public bool isClosed;
        //isOpened bool
        //isClosed bool

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

    public enum PathResult
    {
        SearchFail,
        SearchSuccess,
        GoalUnreachable
    }

    public class GridPathfinding : MonoBehaviour
    {
        private enum SquareCalculationType
        {
            Chebyshev,
            Diagonal,
            DiagonalShortcut,
            Euclidean,
            Manhattan
        }

        private enum SquareTraversalType
        {
            AllNonBlocked,
            NoSharpDiagonals,
            SharpDiagonals
        }

        [SerializeField] private TacticsGrid _tacticsGrid;

        public bool IncludeDiagonals { get => _includeDiagonals; set => _includeDiagonals = value; }

        private bool _includeDiagonals = false;
        [Tooltip("Formulas for calculating estimated cost of travel from current node to goal.")]
        private SquareCalculationType _squareHeuristicCost;
        [Tooltip("Formulas for calculating cost of travel between each node.")]
        private SquareCalculationType _squareTraversalCost;
        [Tooltip("How do we traverse open nodes near and between walls when moving diagonally. Only applies to 8 direction movement on grid")]
        private SquareTraversalType _squareTraversalType;
        [Tooltip("How much does Heuristics effect solution. 1 is balanced G and H. Lower guarantees shortest path at cost of slow processing(Dijkstra). Higher becomes faster with longer path(Greedy Best First)")]
        private float _heuristicScale = 1f;
        [Tooltip("Allow a returned path that does not reach the end goal")]
        private bool _allowPartialSolution = false;
        [Tooltip("Should we revisit closed nodes for a possibly short path at the cost of performance?")]
        private bool _ignoreClosed = true;
        [Tooltip("Should we include the start node in the end result path?")]
        private bool _includeStartNodeInPath = true;

        private PriorityQueue<PathNode> _frontierNodes;
        private Dictionary<GridIndex, PathNode> _nodePool;

        public PathResult FindPath(GridIndex startIndex, GridIndex targetIndex, out List<GridIndex> outPath, bool includeDiagonals = false)
        {
            _includeDiagonals = includeDiagonals;

            outPath = new List<GridIndex>();

            if (!_tacticsGrid.IsIndexValid(startIndex) || !_tacticsGrid.IsIndexValid(targetIndex))
                return PathResult.SearchFail;
            if (startIndex == targetIndex)
                return PathResult.SearchSuccess;

            _nodePool = new Dictionary<GridIndex, PathNode>();
            _frontierNodes = new PriorityQueue<PathNode>();

            float timeStart = Time.realtimeSinceStartup;

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
                processNodes = ProcessSingleNode(targetIndex, ref bestNode, ref bestNodeCost);
            }

            if (bestNodeCost != 0f)
                pathResult = PathResult.GoalUnreachable;

            if (pathResult == PathResult.SearchSuccess || _allowPartialSolution)
            {
                outPath = ConvertPathNodesToIndexes(startNode, bestNode);
                //Debug.Log($"PATHFINDER path length = {outPath.Count()}");
            }

            //Debug.Log($"PATHFINDER SearchRoutine: elapsed time = {(Time.realtimeSinceStartup - timeStart) * 1000f}ms");
            return pathResult;
        }

        private int GetNeighborCount()
        {
            switch (_tacticsGrid.GridShape)
            {
                case GridShape.Square:
                    return _includeDiagonals ? 8 : 4;
                case GridShape.Hexagon:
                    return 6;
                case GridShape.Triangle:
                    return _includeDiagonals ? 3 : 6;
            }
            return 0;
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
                currentNode = _nodePool[currentNode.parent];
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
            _nodePool.TryAdd(index, node);
            return node;
        }

        private float GetHeuristicCost(GridIndex source, GridIndex target)
        {
            return Manhattan(source, target);
        }

        private bool IsTraversalAllowed(PathNode source, GridIndex target)
        {
            //TODO: Add logic for square diagonal traversal
            return true;
        }

        private bool ProcessSingleNode(GridIndex goalNode, ref PathNode bestNode, ref float bestNodeCost)
        {
            PathNode currentNode = _frontierNodes.Dequeue();
            currentNode.isClosed = true;

            if (currentNode.index == goalNode)
            {
                bestNode = currentNode;
                bestNodeCost = 0f;
                return false;
            }

            //List<GridIndex> neighbors = GetNeighborIndexes(currentNode.index, _includeDiagonals);
            for (int i = 0; i < GetNeighborCount(); i++)
            {
                GridIndex neighbor = GetNeighborIndex(currentNode.index, i);

                if (!_tacticsGrid.IsIndexValid(neighbor))
                    continue;

                if (neighbor == currentNode.parent || neighbor == currentNode.index || !IsTraversalAllowed(currentNode, neighbor))
                    continue;

                PathNode neighborNode = null;
                if (_nodePool.TryGetValue(neighbor, out PathNode existingNeighbor))
                    neighborNode = existingNeighbor;
                else
                    neighborNode = CreateAndAddNodeToPool(neighbor);

                if (_ignoreClosed && neighborNode.isClosed)
                    continue;

                float newTraversalCost = GetTraversalCost(currentNode, neighborNode) + currentNode.traversalCost;
                float newHeuristic = GetHeuristicCost(neighborNode.index, goalNode) * _heuristicScale;
                float newTotalCost = newTraversalCost + newHeuristic;

                if (newTotalCost >= neighborNode.totalCost)
                    continue;

                neighborNode.traversalCost = newTraversalCost;
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

        private float GetTraversalCost(PathNode source, PathNode target)
        {
            return Manhattan(source.index, target.index) * target.terrainCost;
            //return GetCalculationFromType(_traversalCost, source, target) + target._terrainCost;
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

        public GridIndex GetNeighborIndex(GridIndex index, int neighborIndex)
        {
            if (GetNeighborIndexes(index).Count > 0)
                return GetNeighborIndexes(index)[neighborIndex];
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

        private List<GridIndex> GetNeighborIndexesForSquare(GridIndex index, bool includeDiagonals)
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

        private List<GridIndex> GetNeighborIndexesForTriangle(GridIndex index, bool includeDiagonals)
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

        private float Chebyshev(PathNode source, PathNode target)
        {
            GridIndex distance = (source.index - target.index).Abs();
            return Mathf.Max(distance.x, distance.z);
        }

        public float Diagonal(PathNode source, PathNode target)
        {
            GridIndex distance = (source.index - target.index).Abs();
            float diagonal = Mathf.Sqrt(2);

            return (distance.x + distance.z) + (diagonal - 2) * Mathf.Min(distance.x, distance.z);
        }

        public float DiagonalShortcut(PathNode source, PathNode target)
        {
            GridIndex distance = (source.index - target.index).Abs();

            return (distance.x + distance.z) + (1.4f - 2) * Mathf.Min(distance.x, distance.z);
        }
        public float Manhattan(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return distance.x + distance.z;
        }

        public float Euclidean(PathNode source, PathNode target)
        {
            GridIndex distance = (source.index - target.index).Abs();

            return Mathf.Sqrt(distance.x * distance.x + distance.z * distance.z);
        }
    }
}