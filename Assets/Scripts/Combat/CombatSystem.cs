using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] private TacticsGrid _tacticsGrid;

    private List<Unit> _unitsInCombat = new List<Unit>();

    private void Start()
    {
        _tacticsGrid.OnGridGenerated += TacticsGrid_OnGridGenerated;
        _tacticsGrid.OnTileDataUpdated += TacticsGrid_OnTileDataUpdated;
    }

    public void AddUnitInCombat(Unit unit, GridIndex gridIndex)
    {
        _unitsInCombat.Add(unit);
        SetUnitIndexOnGrid(unit, gridIndex);
    }

    private void SetUnitIndexOnGrid(Unit unit, GridIndex gridIndex)
    {
        if (unit.UnitGridIndex != gridIndex)
        {
            if (_tacticsGrid.GridTiles.TryGetValue(unit.UnitGridIndex, out TileData prevTile))
            {
                if (prevTile.unitOnTile == unit)
                {
                    prevTile.unitOnTile = null;
                    _tacticsGrid.GridTiles[unit.UnitGridIndex] = prevTile;
                }
            }
        }

        Vector3 newUnitPosition = new Vector3(int.MinValue, 0f, int.MinValue);
        if (gridIndex != new GridIndex(int.MinValue, int.MinValue))
        {
            if (_tacticsGrid.GridTiles.TryGetValue(gridIndex, out TileData newTile))
            {
                newTile.unitOnTile = unit;
                unit.UnitGridIndex = gridIndex;

                _tacticsGrid.GridTiles[gridIndex] = newTile;

                newUnitPosition = newTile.tileMatrix.GetPosition();
            }
        }
        unit.transform.position = newUnitPosition;
    }

    public void RemoveUnitFromCombat(Unit unit, bool shouldDestroy = true)
    {
        _unitsInCombat.Remove(unit);

        if (shouldDestroy)
            Destroy(unit.gameObject);
        else
            SetUnitIndexOnGrid(unit, new GridIndex(int.MinValue, int.MaxValue));
    }

    private void TacticsGrid_OnGridGenerated()
    {
        List<Unit> copyList = new List<Unit>(_unitsInCombat);
        for (int i = 0; i < copyList.Count; i++)
        {
            Unit unit = copyList[i];
            GridIndex unitIndex = unit.UnitGridIndex;
            if (_tacticsGrid.IsTileWalkable(unitIndex))
            {
                SetUnitIndexOnGrid(copyList[i], unitIndex);
            }
            else
            {
                RemoveUnitFromCombat(copyList[i]);
            }
        }
    }

    private void TacticsGrid_OnTileDataUpdated(GridIndex index)
    {
        List<Unit> copyList = new List<Unit>(_unitsInCombat);
        for (int i = 0; i < copyList.Count; i++)
        {
            if (copyList[i].UnitGridIndex == index)
            {
                if (_tacticsGrid.IsTileWalkable(index))
                {
                    SetUnitIndexOnGrid(copyList[i], index);
                }
                else
                {
                    RemoveUnitFromCombat(copyList[i]);
                }
                return;
            }
        }
    }
}
