using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public event Action<Unit, GridIndex> OnUnitGridIndexChanged;

    [SerializeField] private TacticsGrid _tacticsGrid;

    private List<Unit> _unitsInCombat = new List<Unit>();

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

    public void AddUnitToCombat(GridIndex gridIndex, Unit unit)
    {
        _unitsInCombat.Add(unit);
        _tacticsGrid.AddUnitToTile(gridIndex, unit);
    }

    public void AddUnitToCombat(Vector3 worldPosition, Unit unit)
    {
        _unitsInCombat.Add(unit);
        GridIndex unitIndex = _tacticsGrid.GetTileIndexFromWorldPosition(worldPosition);
        if (_tacticsGrid.AddUnitToTile(unitIndex, unit))
        {
            //Success. Add additional logic in here as project requires.
        }
    }

    /// <summary>
    /// Remove unit from combat and place at a desired position. Removes unit from grid.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="newPosition"></param>
    public void RemoveUnitFromCombat(Unit unit, Vector3 newPosition)
    {
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
            unit.GetComponent<IUnitAnimation>().PlayDeathAnimation();
        }

    }

    public bool TryActivateAbility(Ability ability, GridIndex origin, GridIndex target)
    {
        Ability abilityObject = Instantiate(ability, _tacticsGrid.GetWorldPositionFromGridIndex(origin), Quaternion.identity);
        abilityObject.InitializeAbility(_tacticsGrid, origin, target);
        return abilityObject.TryActivateAbility();
        //GetDataFromAbility
        //InstantiateAbility
        // So Abilities should probably have an initialize to pass start, goal, spawn particles, play sound, etc...
    }

    public bool IsValidTileForUnit(Unit unit, GridIndex index)
    {
        if (!_tacticsGrid.IsIndexValid(index))
            return false;

        List<TileType> tileTypes = unit.UnitData.unitStats.validTileTypes;
        return tileTypes != null && tileTypes.Contains(_tacticsGrid.GridTiles[index].tileType);
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
        Unit unit = _unitsInCombat.FirstOrDefault<Unit>(u => u.UnitGridIndex == index);
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

    public List<GridIndex> GetAbilityRange(GridIndex originIndex)
    {
        return new List<GridIndex>();
    }

    public static List<GridIndex> GetIndexesFromPatternAndRange(GridIndex origin, GridShape gridShape, AbilityRangePattern pattern, Vector2Int rangeMinMax)
    {


        return new List<GridIndex>();
    }
}
