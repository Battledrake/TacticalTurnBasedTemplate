using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class GridStatics
    {
        private static GridIndex[] SquareNeighbors = new GridIndex[]
        {
            new GridIndex(1, 0),
            new GridIndex(0, 1),
            new GridIndex(-1, 0),
            new GridIndex(0, -1),
            new GridIndex(1, 1),
            new GridIndex(-1, 1),
            new GridIndex(-1, -1),
            new GridIndex(1, -1)
        };

        private static GridIndex[] HexagonEvenRowNeighbors = new GridIndex[]
        {
            new GridIndex(-1, 0),
            new GridIndex(1, 0),
            new GridIndex(-1, 1),
            new GridIndex(0, 1),
            new GridIndex(-1, -1),
            new GridIndex(0, -1)
        };

        private static GridIndex[] HexagonOddRowNeighbors = new GridIndex[]
        {
            new GridIndex(-1, 0),
            new GridIndex(1, 0),
            new GridIndex(1, 1),
            new GridIndex(0, 1),
            new GridIndex(1, -1),
            new GridIndex(0, -1)
        };

        private static GridIndex[] TriangleFacingDownNeighbors = new GridIndex[]
{
            new GridIndex(-1, 0),
            new GridIndex(0, 1),
            new GridIndex(1, 0),
            new GridIndex(-2, 1),
            new GridIndex(0, -1),
            new GridIndex(2, 1),
};

        private static GridIndex[] TriangleFacingUpNeighbors = new GridIndex[]
        {
            new GridIndex(-1, 0),
            new GridIndex(0, -1),
            new GridIndex(1, 0),
            new GridIndex(-2, -1),
            new GridIndex(0, 1),
            new GridIndex(2, -1),
        };


        public static GridIndex GetSquareNeighborAtIndex(GridIndex gridIndex, int neighborIndex)
        {
            return gridIndex + SquareNeighbors[neighborIndex];
        }

        public static GridIndex GetHexagonNeighborAtIndex(GridIndex gridIndex, int neighborIndex)
        {
            if (gridIndex.z % 2 == 1)
                return gridIndex + HexagonOddRowNeighbors[neighborIndex];
            else
                return gridIndex + HexagonEvenRowNeighbors[neighborIndex];
        }

        public static GridIndex GetTriangleNeighborAtIndex(GridIndex gridIndex, int neighborIndex)
        {
            if (IsTriangleTileFacingUp(gridIndex))
                return gridIndex + TriangleFacingUpNeighbors[neighborIndex];
            else
                return gridIndex + TriangleFacingDownNeighbors[neighborIndex];
        }

        public static Vector3 SnapVectorToVector(Vector3 vectorToSnap, Vector3 snapToVector)
        {
            return new Vector3(
                Mathf.Round(vectorToSnap.x / snapToVector.x) * snapToVector.x,
                Mathf.Round(vectorToSnap.y / snapToVector.y) * snapToVector.y,
                Mathf.Round(vectorToSnap.z / snapToVector.z) * snapToVector.z
                );
        }

        public static bool IsFloatEven(float value)
        {
            return value % 2 == 0;
        }

        public static bool IsTriangleTileFacingUp(GridIndex gridIndex)
        {
            return (gridIndex.x + gridIndex.z) % 2 == 0;
        }

        public static bool IsTileTypeWalkable(TileType tileType)
        {
            return tileType != TileType.None && tileType != TileType.Obstacle;
        }
    }
}
