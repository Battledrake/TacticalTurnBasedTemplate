using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class GridPathfinding : MonoBehaviour
    {
        [SerializeField] private TacticsGrid _tacticsGrid;

        public List<Vector2Int> GetValidTileNeighbors(Vector2Int index)
        {
            switch (_tacticsGrid.GridShape)
            {
                case GridShape.Square:
                    return GetNeighborIndexesForSquare(index);
                case GridShape.Hexagon:
                    break;
                case GridShape.Triangle:
                    break;
            }
            return new List<Vector2Int>();
        }

        private List<Vector2Int> GetNeighborIndexesForSquare(Vector2Int index)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>
            {
                index + new Vector2Int(1, 0),
                index + new Vector2Int(0, 1),
                index + new Vector2Int(-1, 0),
                index + new Vector2Int(0, -1)
            };
            return neighbors;
        }
    }
}