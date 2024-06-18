using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class AbilityStatics
    {
        public static bool HasLineOfSight(TileData originData, TileData targetData, float height, float offsetDistance)
        {
            Vector3 startPosition = originData.tileMatrix.GetPosition();
            startPosition.y += height;

            Vector3 targetPosition = targetData.tileMatrix.GetPosition();
            targetPosition.y += height;

            Vector3 direction = targetPosition - startPosition;

            LayerMask checkLayers = LayerMask.GetMask("Ground", "Obstacles", "Unit");

            if (Physics.Raycast(startPosition, direction, out RaycastHit hitInfo, direction.magnitude, checkLayers))
            {

                Unit abilityUnit = originData.unitOnTile;
                Unit targetUnit = targetData.unitOnTile;
                Unit hitUnit = hitInfo.collider.GetComponent<Unit>();
                if (hitUnit != null)
                {
                    if (hitUnit != abilityUnit && hitUnit != targetUnit)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (offsetDistance > 0)
                    {
                        //Offset distances are scaled from 0 to 1, reflecting percentage from center to edge. We math that here.
                        float relativeDistance = (originData.tileMatrix.lossyScale.x / 2) * offsetDistance;
                        Vector2[] offsets = new Vector2[]
                        {
                        new Vector2(-relativeDistance, 0f),
                        new Vector2(0f, relativeDistance),
                        new Vector2(relativeDistance, 0f),
                        new Vector2(0f, -relativeDistance)
                        };
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            Vector3 startOffset = startPosition + new Vector3(offsets[i].x, 0f, offsets[i].y);

                            int unitLayer = 0;
                            if (abilityUnit)
                            {
                                unitLayer = abilityUnit.gameObject.layer;
                                abilityUnit.gameObject.layer = LayerMask.GetMask("Ignore Raycast");
                            }

                            if (!Physics.Raycast(startOffset, direction, out hitInfo, direction.magnitude, checkLayers))
                            {
                                if (abilityUnit)
                                    abilityUnit.gameObject.layer = unitLayer;
                                return true;
                            }
                            else
                            {
                                if (hitUnit = hitInfo.collider.GetComponent<Unit>())
                                {
                                    if (abilityUnit)
                                        abilityUnit.gameObject.layer = unitLayer;

                                    if (hitUnit != targetUnit)
                                        return false;
                                    else
                                        return true;
                                }
                            }
                            if (abilityUnit)
                                abilityUnit.gameObject.layer = unitLayer;
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        public static List<GridIndex> GetIndexesFromPatternAndRange(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax, AbilityRangePattern pattern)
        {
            List<GridIndex> patternList = new List<GridIndex>();
            switch (pattern)
            {
                case AbilityRangePattern.None:
                    patternList.Add(origin);
                    return patternList;
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

        private static List<GridIndex> OffsetIndexArray(List<GridIndex> indexList, GridIndex offset)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            indexList.ForEach(i => returnList.Add(i + offset));
            return returnList;
        }

        private static List<GridIndex> GetLinePattern(GridIndex origin, GridShape shape, Vector2Int rangeMinMax)
        {
            HashSet<GridIndex> returnSet = new HashSet<GridIndex>();
            for (int i = rangeMinMax.x; i <= rangeMinMax.y; i++)
            {
                switch (shape)
                {
                    case GridShape.Square:
                        returnSet.Add(new GridIndex(i, 0));
                        returnSet.Add(new GridIndex(-i, 0));
                        returnSet.Add(new GridIndex(0, i));
                        returnSet.Add(new GridIndex(0, -i));
                        break;

                    case GridShape.Hexagon:
                        returnSet.Add(new GridIndex(-i, 0));
                        returnSet.Add(new GridIndex(i, 0));

                        int negX = origin.z % 2 == 0 ? Mathf.FloorToInt(-i / 2f) : Mathf.CeilToInt(-i / 2f);
                        int posX = origin.z % 2 == 0 ? Mathf.FloorToInt(i / 2f) : Mathf.CeilToInt(i / 2f);

                        returnSet.Add(new GridIndex(posX, i));
                        returnSet.Add(new GridIndex(negX, i));
                        returnSet.Add(new GridIndex(posX, -i));
                        returnSet.Add(new GridIndex(negX, -i));

                        break;

                    case GridShape.Triangle:
                        returnSet.Add(new GridIndex(i, 0));
                        returnSet.Add(new GridIndex(-i, 0));

                        if (GridStatics.IsTriangleTileFacingUp(origin))
                        {
                            returnSet.Add(new GridIndex(Mathf.CeilToInt(i * 0.5f), Mathf.FloorToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.CeilToInt(i * 0.5f), Mathf.FloorToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(Mathf.CeilToInt(i * -0.5f), Mathf.FloorToInt(i * -0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.CeilToInt(i * -0.5f), Mathf.FloorToInt(i * -0.5f)));
                        }
                        else
                        {
                            returnSet.Add(new GridIndex(Mathf.FloorToInt(i * 0.5f), Mathf.CeilToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.FloorToInt(i * 0.5f), Mathf.CeilToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(Mathf.FloorToInt(i * -0.5f), Mathf.CeilToInt(i * -0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.FloorToInt(i * -0.5f), Mathf.CeilToInt(i * -0.5f)));
                        }
                        break;
                }
            }
            return returnSet.ToList();
        }

        private static List<GridIndex> GetDiagonalPattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            HashSet<GridIndex> returnSet = new HashSet<GridIndex>();
            for (int i = rangeMinMax.x; i <= rangeMinMax.y; i++)
            {
                switch (gridShape)
                {

                    case GridShape.Square:
                        returnSet.Add(new GridIndex(i, i));
                        returnSet.Add(new GridIndex(-i, -i));
                        returnSet.Add(new GridIndex(-i, i));
                        returnSet.Add(new GridIndex(i, -i));
                        break;

                    case GridShape.Hexagon:
                        returnSet.Add(new GridIndex(0, i * 2)); //up
                        returnSet.Add(new GridIndex(0, -i * 2)); // down

                        int negX = origin.z % 2 == 0 ? Mathf.FloorToInt(-i * 1.5f) : Mathf.CeilToInt(-i * 1.5f);
                        int posX = origin.z % 2 == 0 ? Mathf.FloorToInt(i * 1.5f) : Mathf.CeilToInt(i * 1.5f);

                        returnSet.Add(new GridIndex(posX, i)); //up right
                        returnSet.Add(new GridIndex(negX, i)); //up left
                        returnSet.Add(new GridIndex(posX, -i)); //Down Right
                        returnSet.Add(new GridIndex(negX, -i)); //Down Left
                        break;

                    case GridShape.Triangle:
                        returnSet.Add(new GridIndex(0, i));
                        returnSet.Add(new GridIndex(0, -i));

                        if (GridStatics.IsTriangleTileFacingUp(origin))
                        {
                            returnSet.Add(new GridIndex(Mathf.FloorToInt(i * 0.5f * 3.0f), Mathf.FloorToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.FloorToInt(i * 0.5f * 3.0f), Mathf.FloorToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(Mathf.FloorToInt(i * -0.5f * 3.0f), Mathf.FloorToInt(i * -0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.FloorToInt(i * -0.5f * 3.0f), Mathf.FloorToInt(i * -0.5f)));
                        }
                        else
                        {
                            returnSet.Add(new GridIndex(Mathf.CeilToInt(i * 0.5f * 3.0f), Mathf.CeilToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.CeilToInt(i * 0.5f * 3.0f), Mathf.CeilToInt(i * 0.5f)));
                            returnSet.Add(new GridIndex(Mathf.CeilToInt(i * -0.5f * 3.0f), Mathf.CeilToInt(i * -0.5f)));
                            returnSet.Add(new GridIndex(-Mathf.CeilToInt(i * -0.5f * 3.0f), Mathf.CeilToInt(i * -.5f)));
                        }
                        break;
                }
            }
            return returnSet.ToList();
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

        private static List<GridIndex> GetSquarePattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            HashSet<GridIndex> returnSet = new HashSet<GridIndex>();
            for (int i = rangeMinMax.x; i <= rangeMinMax.y; i++)
            {
                switch (gridShape)
                {
                    case GridShape.Square:
                        for (int j = -i; j <= i; j++)
                        {
                            returnSet.Add(new GridIndex(-i, j));
                            returnSet.Add(new GridIndex(j, i));
                            returnSet.Add(new GridIndex(i, -j));
                            returnSet.Add(new GridIndex(-j, -i));
                        }
                        break;
                    case GridShape.Hexagon:
                        for (int j = -i; j <= i; j++)
                        {
                            if (i != 0)
                            {
                                returnSet.Add(new GridIndex(i, j));
                                returnSet.Add(new GridIndex(-i, j));
                            }
                            if (i != j)
                            {
                                returnSet.Add(new GridIndex(j, i));
                                returnSet.Add(new GridIndex(j, -i));
                            }
                        }
                        break;
                    case GridShape.Triangle:
                        for (int j = -i; j <= i; j++)
                        {
                            returnSet.Add(new GridIndex(-i * 2, j)); //Down To up, Left
                            returnSet.Add(new GridIndex(i * 2, -j)); //Up to down, right
                            if (i != j)
                            {
                                returnSet.Add(new GridIndex(-i * 2 + 1, j)); //down to up, left
                                returnSet.Add(new GridIndex(i * 2 - 1, -j)); //up to down, right
                            }
                        }
                        for (int j = -i * 2; j <= i * 2; j++)
                        {
                            returnSet.Add(new GridIndex(j, i)); //Up, Left to Right
                            returnSet.Add(new GridIndex(-j, -i)); //Down, right to left
                        }
                        break;
                }
            }
            return returnSet.ToList();
        }



        private static List<GridIndex> GetDiamondPattern(GridIndex origin, GridShape gridShape, Vector2Int rangeMinMax)
        {
            HashSet<GridIndex> returnSet = new HashSet<GridIndex>();

            for (int i = rangeMinMax.x; i <= rangeMinMax.y; i++)
            {
                switch (gridShape)
                {
                    case GridShape.Square:
                        for (int j = 0; j <= i; j++)
                        {
                            returnSet.Add(new GridIndex(-(i - j), j));
                            returnSet.Add(new GridIndex(j, i - j));
                            returnSet.Add(new GridIndex(i - j, -j));
                            returnSet.Add(new GridIndex(-j, -(i - j)));
                        }
                        break;
                    case GridShape.Hexagon:
                        int hX = 0;
                        int hZ = i;

                        for (int side = 0; side < 6; side++)
                        {
                            for (int j = 0; j < i; j++)
                            {
                                if (side == 0) { hX++; hZ--; } // Move right-up
                                else if (side == 1) { hZ--; } // Move up
                                else if (side == 2) { hX--; } // Move left-up
                                else if (side == 3) { hX--; hZ++; } // Move left-down
                                else if (side == 4) { hZ++; } // Move down
                                else if (side == 5) { hX++; } // Move right-down

                                // Convert axial to odd-r offset
                                int col = origin.z % 2 == 0 ? hX + (hZ - (hZ & 1)) / 2 : hX + (hZ + (hZ & 1)) / 2;

                                int row = hZ;

                                returnSet.Add(new GridIndex(col, row));
                            }
                        }
                        break;
                    case GridShape.Triangle:
                        bool isFacingUp = GridStatics.IsTriangleTileFacingUp(origin);
                        for (int j = 0; j <= i; j++)
                        {
                            int z = isFacingUp ? j : -j;
                            int x = i * 2 - j;

                            returnSet.Add(new GridIndex(-x, z));
                            returnSet.Add(new GridIndex(x, -z));

                            if (j != i)
                                returnSet.Add(new GridIndex(-x + 1, z));

                            if (j != 0)
                                returnSet.Add(new GridIndex(x + 1, -z));


                            z = isFacingUp ? i - j : -(i - j);
                            x = (i * 2) - (i - j);

                            returnSet.Add(new GridIndex(-x, -z));
                            returnSet.Add(new GridIndex(x, z));

                            if (j != i)
                                returnSet.Add(new GridIndex(-x - 1, -z));

                            if (j != 0)
                                returnSet.Add(new GridIndex(x - 1, z));

                        }
                        for (int j = -i; j <= i; j++)
                        {
                            returnSet.Add(new GridIndex(j, isFacingUp ? i : -i));
                            returnSet.Add(new GridIndex(-j, isFacingUp ? -i : i));
                        }
                        break;
                }
            }
            return returnSet.ToList();
        }
    }
}
