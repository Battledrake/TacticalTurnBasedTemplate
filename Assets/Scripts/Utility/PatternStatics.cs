using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineFreeLook;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class PatternStatics
    {
        public static List<GridIndex> GetIndexesFromPatternAndRange(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax, AbilityRangePattern pattern)
        {
            List<GridIndex> patternList = new List<GridIndex>();
            switch (pattern)
            {
                case AbilityRangePattern.None:
                    patternList.Add(origin);
                    break;
                case AbilityRangePattern.Line:
                    patternList = GetLinePattern(origin, gridShape, rangeMinMax);
                    break;
                case AbilityRangePattern.Diagonal:
                    patternList = GetDiagonalPattern(origin, gridShape, rangeMinMax);
                    break;
                case AbilityRangePattern.HalfDiagonal:
                    patternList = GetHalfDiagonalPattern(origin, gridShape, rangeMinMax);
                    break;
                case AbilityRangePattern.Star:
                    patternList = GetStarPattern(origin, gridShape, rangeMinMax);
                    break;
                case AbilityRangePattern.Diamond:
                    patternList = GetDiamondPattern(origin, gridShape, rangeMinMax);
                    break;
                case AbilityRangePattern.Square:
                    patternList = GetSquarePattern(origin, gridShape, rangeMinMax);
                    break;
            }
            return OffsetIndexArray(patternList, origin);
        }

        public static List<GridIndex> OffsetIndexArray(List<GridIndex> indexList, GridIndex offset)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            indexList.ForEach(i => returnList.Add(i + offset));
            return returnList;
        }

        public static List<GridIndex> GetLinePattern(GridIndex origin, GridShape shape, Vector2Int rangeMinMax)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            for (int i = rangeMinMax.x; i < rangeMinMax.y; i++)
            {
                switch (shape)
                {
                    case GridShape.Square:
                        returnList.Add(new GridIndex(i, 0));
                        returnList.Add(new GridIndex(-i, 0));
                        returnList.Add(new GridIndex(0, i));
                        returnList.Add(new GridIndex(0, -i));
                        break;

                    case GridShape.Hexagon:
                        bool isOddRow = origin.z % 2 == 1;
                        returnList.Add(new GridIndex(-i, 0)); //Left
                        returnList.Add(new GridIndex(isOddRow ? 0 : -i, i)); //Up Left
                        returnList.Add(new GridIndex(isOddRow ? i : 0, i)); //Up Right
                        returnList.Add(new GridIndex(i, 0)); //Right
                        returnList.Add(new GridIndex(isOddRow ? i : 0, -i)); //Down Right
                        returnList.Add(new GridIndex(isOddRow ? 0 : -i, -i)); //Down Left
                        break;

                    case GridShape.Triangle:
                        returnList.Add(new GridIndex(i, 0));
                        returnList.Add(new GridIndex(-i, 0));

                        if (GridStatics.IsTriangleTileFacingUp(origin))
                        {
                            returnList.Add(new GridIndex(Mathf.CeilToInt(i * 0.5f), Mathf.FloorToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(-Mathf.CeilToInt(i * 0.5f), Mathf.FloorToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(Mathf.CeilToInt(i * -0.5f), Mathf.FloorToInt(i * -0.5f)));
                            returnList.Add(new GridIndex(-Mathf.CeilToInt(i * -0.5f), Mathf.FloorToInt(i * -0.5f)));
                        }
                        else
                        {
                            returnList.Add(new GridIndex(Mathf.FloorToInt(i * 0.5f), Mathf.CeilToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(-Mathf.FloorToInt(i * 0.5f), Mathf.CeilToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(Mathf.FloorToInt(i * -0.5f), Mathf.CeilToInt(i * -0.5f)));
                            returnList.Add(new GridIndex(-Mathf.FloorToInt(i * -0.5f), Mathf.CeilToInt(i * -0.5f)));
                        }
                        break;
                }
            }
            return returnList;
        }

        public static List<GridIndex> GetDiagonalPattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            for (int i = rangeMinMax.x; i < rangeMinMax.y; i++)
            {
                switch (gridShape)
                {

                    case GridShape.Square:
                        returnList.Add(new GridIndex(i, i));
                        returnList.Add(new GridIndex(-i, -i));
                        returnList.Add(new GridIndex(-i, i));
                        returnList.Add(new GridIndex(i, -i));
                        break;

                    case GridShape.Hexagon:
                        returnList.Add(new GridIndex(0, i * 2)); //up
                        returnList.Add(new GridIndex(0, -i * 2)); // down
                        returnList.Add(new GridIndex(Mathf.CeilToInt(i * 3), i)); //up right
                        returnList.Add(new GridIndex(-Mathf.CeilToInt(i * 3), i)); //up left
                        returnList.Add(new GridIndex(Mathf.CeilToInt(i * 3), -i)); //Down Right
                        returnList.Add(new GridIndex(-Mathf.CeilToInt(i * 3), -i)); //Down Left
                        break;

                    case GridShape.Triangle:
                        returnList.Add(new GridIndex(0, i));
                        returnList.Add(new GridIndex(0, -i));

                        if (GridStatics.IsTriangleTileFacingUp(origin))
                        {
                            returnList.Add(new GridIndex(Mathf.FloorToInt(i * 0.5f * 3.0f), Mathf.FloorToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(-Mathf.FloorToInt(i * 0.5f * 3.0f), Mathf.FloorToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(Mathf.FloorToInt(i * -0.5f * 3.0f), Mathf.FloorToInt(i * -0.5f)));
                            returnList.Add(new GridIndex(-Mathf.FloorToInt(i * -0.5f * 3.0f), Mathf.FloorToInt(i * -0.5f)));
                        }
                        else
                        {
                            returnList.Add(new GridIndex(Mathf.CeilToInt(i * 0.5f * 3.0f), Mathf.CeilToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(-Mathf.CeilToInt(i * 0.5f * 3.0f), Mathf.CeilToInt(i * 0.5f)));
                            returnList.Add(new GridIndex(Mathf.CeilToInt(i * -0.5f * 3.0f), Mathf.CeilToInt(i * -0.5f)));
                            returnList.Add(new GridIndex(-Mathf.CeilToInt(i * -0.5f * 3.0f), Mathf.CeilToInt(i * -.5f)));
                        }
                        break;
                }
            }
            return returnList;
        }

        public static List<GridIndex> GetHalfDiagonalPattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            return GetDiagonalPattern(origin, gridShape, rangeMinMax / 2);
        }

        public static List<GridIndex> GetStarPattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            List<GridIndex> returnList = GetLinePattern(origin, gridShape, rangeMinMax);

            if (gridShape == GridShape.Square)
            {
                GetDiagonalPattern(origin, gridShape, rangeMinMax).ForEach(i => returnList.Add(i));
                return returnList;
            }
            else
            {
                GetHalfDiagonalPattern(origin, gridShape, rangeMinMax).ForEach(i => returnList.Add(i));
                return returnList;
            }
        }

        public static List<GridIndex> GetSquarePattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            for (int i = rangeMinMax.x; i < rangeMinMax.y; i++)
            {
                switch (gridShape)
                {
                    case GridShape.Square:
                        for (int j = -i; j < i; j++)
                        {
                            returnList.Add(new GridIndex(-i, j));
                            returnList.Add(new GridIndex(j, i));
                            returnList.Add(new GridIndex(i, -j));
                            returnList.Add(new GridIndex(-j, -i));
                        }
                        break;
                    default:
                        for (int j = -i; j < i; j++)
                        {
                            returnList.Add(new GridIndex(-i * 2, j)); //Down To up, Left
                            returnList.Add(new GridIndex(i * 2, -j)); //Up to down, right
                            if (i != j)
                            {
                                returnList.Add(new GridIndex(-i * 2 + 1, j)); //down to up, left
                                returnList.Add(new GridIndex(i * 2 - 1, -j)); //up to down, right
                            }
                        }
                        for (int j = -i * 2; j < i * 2; j++)
                        {
                            returnList.Add(new GridIndex(j, i)); //Up, Left to Right
                            returnList.Add(new GridIndex(-j, -i)); //Down, right to left
                        }
                        break;
                }
            }
            return returnList;
        }

        public static List<GridIndex> GetDiamondPattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            List<GridIndex> returnList = new List<GridIndex>();

            for (int i = rangeMinMax.x; i < rangeMinMax.y; i++)
            {
                switch (gridShape)
                {
                    case GridShape.Square:
                        for (int j = 0; j < i; j++)
                        {
                            returnList.Add(new GridIndex(-(i - j), j));
                            returnList.Add(new GridIndex(j, i - j));
                            returnList.Add(new GridIndex(i - j, -j));
                            returnList.Add(new GridIndex(-j, -(i - j)));
                        }
                        break;

                    case GridShape.Hexagon:
                        for (int j = 0; j < 0; j++)
                        {
                            returnList.Add(new GridIndex(-(i * 2 - j), j)); //Mid to up, left to right
                            returnList.Add(new GridIndex(i * 2 - j, -j)); //Mid to down, right to left
                            returnList.Add(new GridIndex(i * 2 - (i - j), i - j)); //Up to Mid, left to right
                            returnList.Add(new GridIndex(-i * 2 - (i - j), -(i - j)));
                        }
                        for (int j = -i - 2; j < i - 2; j++)
                        {
                            returnList.Add(new GridIndex(j, i)); //up, midleft to midright
                            returnList.Add(new GridIndex(-j, -i)); //up midleft to midright
                        }
                        break;

                    case GridShape.Triangle:
                        for (int j = 0; j < i; j++)
                        {
                            int z = GridStatics.IsTriangleTileFacingUp(origin) ? j : -j;
                            int x = i * 2 - j;
                            returnList.Add(new GridIndex(-x, z));
                            returnList.Add(new GridIndex(x, -z));
                            if (j != i)
                                returnList.Add(new GridIndex(-x + 1, z));
                            if (j != 0)
                                returnList.Add(new GridIndex(x + 1, -z));

                            z = GridStatics.IsTriangleTileFacingUp(origin) ? i - j : -(i - j);
                            x = i * 2 - (i - j);

                            returnList.Add(new GridIndex(-x, -z));
                            returnList.Add(new GridIndex(x, z));
                            if (i != j)
                                returnList.Add(new GridIndex(-x - 1, -z));
                            if (j != 0)
                                returnList.Add(new GridIndex(x - 1, z));

                        }
                        for (int j = -i; j < i; j++)
                        {
                            returnList.Add(new GridIndex(j, GridStatics.IsTriangleTileFacingUp(origin) ? i : -i));
                            returnList.Add(new GridIndex(-j, GridStatics.IsTriangleTileFacingUp(origin) ? -i : i));
                        }
                        break;
                }
            }
            return returnList;
        }
    }
}