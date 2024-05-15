using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AddUnitToGridAction : ActionBase
    {
        [SerializeField] private Unit _unitPrefab;
        public override bool ExecuteAction(GridIndex index)
        {
            if (actionValue < 0)
                return false;

            if (_playerActions.TacticsGrid.IsTileWalkable(index))
            {
                _playerActions.TacticsGrid.GridTiles.TryGetValue(index, out TileData tileData);
                if (!tileData.unitOnTile)
                {
                    List<TileType> validTileTypes = DataManager.GetUnitDataFromType((UnitType)actionValue).unitStats.validTileTypes;

                    if (validTileTypes != null || validTileTypes.Count > 0 || validTileTypes.Contains(tileData.tileType))
                    {
                        Unit newUnit = Instantiate(_unitPrefab);
                        newUnit.gameObject.name = ((UnitType)actionValue).ToString();
                        newUnit.InitializeUnit((UnitType)actionValue);
                        newUnit.SetUnitsGrid(_playerActions.TacticsGrid);

                        _playerActions.CombatSystem.AddUnitToCombat(index, newUnit);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}