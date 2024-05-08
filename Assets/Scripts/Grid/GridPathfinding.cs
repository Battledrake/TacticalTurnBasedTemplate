using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class GridPathfinding : MonoBehaviour
    {
        [SerializeField] private TacticsGrid _tacticsGrid;

        public bool IncludeDiagonals { get => _includeDiagonals; set => _includeDiagonals = value; }

        private bool _includeDiagonals = false;

        public List<Vector2Int> GetValidTileNeighbors(Vector2Int index, bool includeDiagonals = false)
        {
            List<Vector2Int> neighborIndexes = GetNeighborIndexes(index, includeDiagonals);
            List<Vector2Int> validNeighbors = new List<Vector2Int>();
            _tacticsGrid.GridTiles.TryGetValue(index, out TileData selectedTile);

            for(int i = 0; i < neighborIndexes.Count; i++)
            {
                if(_tacticsGrid.GridTiles.TryGetValue(neighborIndexes[i], out TileData tileData))
                {
                    if (GridStatics.IsTileTypeWalkable(tileData.tileType))
                    {
                        float heightDifference = Mathf.Abs(tileData.tileMatrix.GetPosition().y - selectedTile.tileMatrix.GetPosition().y);
                        if(heightDifference <= _tacticsGrid.TileSize.y)
                        {
                            validNeighbors.Add(tileData.index);
                        }
                    }
                }
            }
            return validNeighbors;
        }

        public List<Vector2Int> GetNeighborIndexes(Vector2Int index, bool includeDiagonals = false)
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
            return new List<Vector2Int>();
        }

        private List<Vector2Int> GetNeighborIndexesForSquare(Vector2Int index, bool includeDiagonals)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                index + new Vector2Int(1, 0),
                index + new Vector2Int(0, 1),
                index + new Vector2Int(-1, 0),
                index + new Vector2Int(0, -1)
            };

            if (includeDiagonals)
            {
                neighbors.Add(index + new Vector2Int(1, 1));
                neighbors.Add(index + new Vector2Int(-1, 1));
                neighbors.Add(index + new Vector2Int(-1, -1));
                neighbors.Add(index + new Vector2Int(1, -1));
            }
            return neighbors;
        }

        private List<Vector2Int> GetNeighborIndexesForHexagon(Vector2Int index)
        {
            bool isOddRow = index.y % 2 == 1;
            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                index + new Vector2Int(-1, 0),
                index + new Vector2Int(1, 0),
                index + new Vector2Int(isOddRow ? 1 : -1, 1),
                index + new Vector2Int(0, 1),
                index + new Vector2Int(isOddRow ? 1 : -1, -1),
                index + new Vector2Int(0, -1)
            };
            return neighbors;
        }

        private List<Vector2Int> GetNeighborIndexesForTriangle(Vector2Int index, bool includeDiagonals)
        {
            bool isFacingUp = index.x % 2 == index.y % 2;
            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                index + new Vector2Int(-1, 0),
                index + new Vector2Int(0, isFacingUp ? -1 : 1),
                index + new Vector2Int(1, 0)
            };

            if (includeDiagonals)
            {
                neighbors.Add(index + new Vector2Int(-2, isFacingUp ? -1 : 1));
                neighbors.Add(index + new Vector2Int(0, isFacingUp ? 1 : -1));
                neighbors.Add(index + new Vector2Int(2, isFacingUp ? -1 : 1));
            }
            return neighbors;
        }
    }
}