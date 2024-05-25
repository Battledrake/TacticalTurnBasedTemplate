using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor.Playables;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatSystem : MonoBehaviour
    {
        public static CombatSystem Instance;

        public event Action<Unit, GridIndex> OnUnitGridIndexChanged;

        [SerializeField] private bool _drawLineOfSightLines = false;
        [SerializeField] private TacticsGrid _tacticsGrid;

        public List<Unit> UnitsInCombat { get => _unitsInCombat; }

        private List<Unit> _unitsInCombat = new List<Unit>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            _tacticsGrid.OnGridGenerated += TacticsGrid_OnGridGenerated;
            _tacticsGrid.OnTileDataUpdated += TacticsGrid_OnTileDataUpdated;
        }

        private void OnEnable()
        {

            Unit.OnUnitReachedNewTile += Unit_OnUnitReachedNewTile;
        }

        private void OnDisable()
        {
            Unit.OnUnitReachedNewTile -= Unit_OnUnitReachedNewTile;
        }

        private void Unit_OnUnitReachedNewTile(Unit unit, GridIndex index)
        {
            _tacticsGrid.RemoveUnitFromTile(unit.UnitGridIndex);
            _tacticsGrid.AddUnitToTile(index, unit, true);
            OnUnitGridIndexChanged?.Invoke(unit, index);
        }

        public void AddUnitToCombat(Vector3 worldPosition, Unit unit)
        {
            GridIndex unitIndex = _tacticsGrid.GetTileIndexFromWorldPosition(worldPosition);
            AddUnitToCombat(unitIndex, unit);
        }

        public void AddUnitToCombat(GridIndex gridIndex, Unit unit)
        {
            if(_tacticsGrid.AddUnitToTile(gridIndex, unit))
            {
                _unitsInCombat.Add(unit);
                unit.OnUnitDied += Unit_OnUnitDied;
            }
            else
            {
                Debug.LogWarning("Unable to add unit to tile. Invalid index or unit already exists at GridIndex");
            }
        }

        private void Unit_OnUnitDied(Unit unit)
        {
            RemoveUnitFromCombat(unit, false);
        }

        /// <summary>
        /// Remove unit from combat and place at a desired position. Removes unit from grid.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="newPosition"></param>
        public void RemoveUnitFromCombat(Unit unit, Vector3 newPosition)
        {
            unit.OnUnitDied -= Unit_OnUnitDied;

            _unitsInCombat.Remove(unit);
            _tacticsGrid.RemoveUnitFromTile(unit.UnitGridIndex);
            unit.UnitGridIndex = GridIndex.Invalid();

            unit.transform.position = newPosition;
        }

        /// <summary>
        /// Remove unit from combat with optional choice to destroy gameobject. If not destroyed, unit remains at position but is taken off the grid and out of the combat units list.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="shouldDestroy"></param>
        public void RemoveUnitFromCombat(Unit unit, bool shouldDestroy = true)
        {
            _unitsInCombat.Remove(unit);
            _tacticsGrid.RemoveUnitFromTile(unit.UnitGridIndex);

            if (shouldDestroy)
            {
                Destroy(unit.gameObject);
            }
            else
            {
                unit.UnitGridIndex = GridIndex.Invalid();
            }
        }

        public bool TryActivateAbility(Ability ability, Unit instigator, GridIndex origin, GridIndex target)
        {
            if (GetAbilityRange(origin, ability.RangeData).Contains(target))
            {
                Ability abilityObject = Instantiate(ability, _tacticsGrid.GetWorldPositionFromGridIndex(origin), Quaternion.identity);

                List<GridIndex> impactIndexes = new List<GridIndex>();
                if (ability.AreaOfEffectData.rangePattern == AbilityRangePattern.None)
                {
                    impactIndexes.Add(target);
                    abilityObject.InitializeAbility(_tacticsGrid, instigator, origin, target);
                }
                else
                {
                    impactIndexes = GetAbilityRange(target, ability.AreaOfEffectData);
                    abilityObject.InitializeAbility(_tacticsGrid, instigator, origin, target, impactIndexes);
                }
                return abilityObject.TryActivateAbility();
            }
            return false;
        }

        public void ApplyEffectsToUnit(Unit instigator, Unit receiver, List<AbilityEffect> effectsToApply)
        {
            if (receiver == null)
                return;

            List<AbilityEffectReal> effectsRealList = new List<AbilityEffectReal>();
            for (int i = 0; i < effectsToApply.Count; i++)
            {
                AbilityEffectReal effectReal;
                effectReal.attributeType = effectsToApply[i].attributeType;
                effectReal.modifier = StaticUtilities.MinMaxRandom(effectsToApply[i].minMaxModifier);
                effectsRealList.Add(effectReal);
            }
            receiver.ApplyEffects(effectsRealList);
        }

        public bool IsValidTileForUnit(Unit unit, GridIndex index)
        {
            if (!_tacticsGrid.IsIndexValid(index))
                return false;

            List<TileType> tileTypes = unit.UnitData.unitStats.validTileTypes;
            return tileTypes != null && tileTypes.Contains(_tacticsGrid.GridTiles[index].tileType);
        }

        public List<GridIndex> RemoveIndexesWithoutLineOfSight(GridIndex origin, List<GridIndex> tiles, float height)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            for (int i = 0; i < tiles.Count; i++)
            {
                if (HasLineOfSight(origin, tiles[i], height))
                {
                    returnList.Add(tiles[i]);
                }
            }
            return returnList;
        }

        public List<GridIndex> RemoveNotWalkableIndexes(List<GridIndex> targetIndexes)
        {
            List<GridIndex> validIndexes = new List<GridIndex>();
            for (int i = 0; i < targetIndexes.Count; i++)
            {
                if (_tacticsGrid.IsTileWalkable(targetIndexes[i]))
                    validIndexes.Add(targetIndexes[i]);
            }
            return validIndexes;
        }

        public bool HasLineOfSight(GridIndex origin, GridIndex target, float height)
        {
            if (!_tacticsGrid.GetTileDataFromIndex(origin, out TileData originData))
            {
                return false;
            }
            if (!_tacticsGrid.GetTileDataFromIndex(target, out TileData targetData))
            {
                return false;
            }


            Vector3 startPosition = originData.tileMatrix.GetPosition();
            startPosition.y += height;

            Vector3 targetPosition = targetData.tileMatrix.GetPosition();
            targetPosition.y += height;

            Vector3 direction = targetPosition - startPosition;

            //if (_drawLineOfSightLines)
            //{
            //    Debug.DrawLine(startPosition, targetPosition, Color.white, 1f);
            //}

            if (Physics.Raycast(startPosition, direction, out RaycastHit hitInfo, direction.magnitude))
            {
                Unit abilityUnit = originData.unitOnTile;
                Unit targetUnit = targetData.unitOnTile;
                Unit hitUnit = hitInfo.collider.GetComponent<Unit>();
                if (hitUnit != null)
                {
                    if (hitUnit != abilityUnit && hitUnit != targetUnit)
                        return false;
                }
                else
                {
                    //TODO: Logic for doing line of sight checks around corners. Has issue?s with height checks and doesn't account for all directions yet. Will fix later.
                    //if (_tacticsGrid.GridShape == GridShape.Square)
                    //{
                    //    //if x = 0 and y = 1 we want to check -x and x
                    //    //if x = 1 and y = 0 we want to check -y and y
                    //    //if x = 0 and y = -1 we want to check -x and x
                    //    //if x = -1 and y = 0 we want to check -y and y
                    //    Vector3 normalizedDirection = direction.normalized;
                    //    if (normalizedDirection.x != 0)
                    //    {
                    //        _tacticsGrid.GetTileDataFromIndex(new GridIndex(origin.x, origin.z - 1), out TileData negZTile);
                    //        Vector3 negZPosition = negZTile.tileMatrix.GetPosition();
                    //        negZPosition.y += height;
                    //        Vector3 checkDirection = new Vector3(direction.x, height, 0f);

                    //        if (_drawLineOfSightLines)
                    //        {
                    //            Debug.DrawLine(negZPosition, negZPosition + checkDirection, Color.white, 1f);
                    //        }

                    //        if (Physics.Raycast(negZPosition, checkDirection, out RaycastHit negZHit, direction.magnitude))
                    //        {
                    //            _tacticsGrid.GetTileDataFromIndex(new GridIndex(origin.x, origin.z + 1), out TileData posZTile);
                    //            Vector3 posZPosition = posZTile.tileMatrix.GetPosition();
                    //            posZPosition.y += height;

                    //            if (Physics.Raycast(posZPosition, checkDirection, out RaycastHit posZHit, direction.magnitude))
                    //            {
                    //                //do another ray for the other tile.
                    //                return false;
                    //            }
                    //            return true;
                    //        }
                    //        return true;
                    //    }
                    //}
                    return false;
                }
            }
            return true;
        }

        private void TacticsGrid_OnGridGenerated()
        {
            List<Unit> copyList = new List<Unit>(_unitsInCombat);
            for (int i = 0; i < copyList.Count; i++)
            {
                Unit unit = copyList[i];
                //GridIndex unitIndex = unit.UnitGridIndex;
                GridIndex positionIndex = _tacticsGrid.GetTileIndexFromWorldPosition(unit.transform.position);
                if (IsValidTileForUnit(unit, positionIndex))
                {
                    _tacticsGrid.AddUnitToTile(positionIndex, unit);
                }
                else
                {
                    RemoveUnitFromCombat(copyList[i], false);
                }
            }
        }

        private void TacticsGrid_OnTileDataUpdated(GridIndex index)
        {
            Unit unit = _unitsInCombat.FirstOrDefault(u => u.UnitGridIndex == index);
            if (unit)
            {
                if (IsValidTileForUnit(unit, index))
                {
                    _tacticsGrid.GetTileDataFromIndex(index, out TileData tileData);
                    unit.transform.position = new Vector3(unit.transform.position.x, tileData.tileMatrix.GetPosition().y, unit.transform.position.z);
                }
                else
                {
                    RemoveUnitFromCombat(unit, false);
                }
            }
        }

        public List<GridIndex> GetAbilityRange(GridIndex originIndex, AbilityRangeData rangeData)
        {
            List<GridIndex> indexesInRange = RemoveNotWalkableIndexes(AbilityStatics.GetIndexesFromPatternAndRange(originIndex, _tacticsGrid.GridShape, rangeData.rangeMinMax, rangeData.rangePattern));
            if (rangeData.lineOfSightData.requireLineOfSight)
            {
                indexesInRange = RemoveIndexesWithoutLineOfSight(originIndex, indexesInRange, rangeData.lineOfSightData.height);
            }
            return indexesInRange;
        }
    }
}