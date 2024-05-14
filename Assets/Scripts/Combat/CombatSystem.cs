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
        _tacticsGrid.AddUnitToTile(index, unit, false);
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

    private void TacticsGrid_OnGridGenerated()
    {
        List<Unit> copyList = new List<Unit>(_unitsInCombat);
        for (int i = 0; i < copyList.Count; i++)
        {
            Unit unit = copyList[i];
            //GridIndex unitIndex = unit.UnitGridIndex;
            GridIndex positionIndex = _tacticsGrid.GetTileIndexFromWorldPosition(unit.transform.position);
            if (_tacticsGrid.IsTileWalkable(positionIndex))
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
            if (_tacticsGrid.IsTileWalkable(index))
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
}
