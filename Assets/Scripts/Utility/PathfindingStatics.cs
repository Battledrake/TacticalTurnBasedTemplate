using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class PathfindingStatics
    {

        public static float GetTerrainCostFromTileType(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.DoubleCost:
                    return 2f;
                case TileType.TripleCost:
                    return 3f;
            }
            return 1f;
        }

        public static GridIndex ConvertOddrToAxial(GridIndex hex)
        {
            var q = hex.x - (hex.z - (hex.z & 1)) / 2;
            var r = hex.z;
            return new GridIndex(q, r);
        }

        public static float GetDistanceFromAxialCoordinates(GridIndex source, GridIndex target)
        {
            int dq = Mathf.Abs(source.x - target.x);
            int dr = Mathf.Abs(source.z - target.z);
            int ds = Mathf.Abs((source.x + source.z) - (target.x + target.z));

            return Mathf.Max(dq, dr, ds);
        }

        public static float GetTriangleDistance(GridIndex source, GridIndex target)
        {
            float triangleHeight = Mathf.Sqrt(3) / 2;

            float x1 = source.x + (source.z & 1) * 0.5f;
            float y1 = source.z * triangleHeight;
            float x2 = target.x + (target.z & 1) * 0.5f;
            float y2 = target.z * triangleHeight;

            return Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
        }

        //Chess style
        public static float GetChebyshevDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();
            return Mathf.Max(distance.x, distance.z);
        }

        //Common diagonal allowing square grid heuristic
        public static float GetDiagonalDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();
            float diagonal = Mathf.Sqrt(2);

            return (distance.x + distance.z) + (diagonal - 2) * Mathf.Min(distance.x, distance.z);
        }

        //Same as Diagonal but does 1.4 instead of sqrt(2). Less accurate, more performant.
        public static float GetDiagonalShortcutDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return (distance.x + distance.z) + (1.4f - 2) * Mathf.Min(distance.x, distance.z);
        }

        //Standard algorithm for 4 neighbor squares and Hexagons.
        public static float GetManhattanDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return distance.x + distance.z;
        }

        //Straight line from start to end calculation
        public static float GetEuclideanDistance(GridIndex source, GridIndex target)
        {
            GridIndex distance = (source - target).Abs();

            return Mathf.Sqrt(distance.x * distance.x + distance.z * distance.z);
        }
    }
}