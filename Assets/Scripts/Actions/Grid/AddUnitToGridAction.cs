using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AddUnitToGridAction : ActionBase
    {
        [SerializeField] private Unit _unitPrefab;

        public int UnitTeamIndex { get => _unitTeamIndex; set => _unitTeamIndex = value; }

        private int _unitTeamIndex = 0;

        public override bool ExecuteAction(GridIndex index)
        {
            if (actionValue < 0)
                return false;

            if (_playerActions.TacticsGrid.IsTileWalkable(index))
            {
                _playerActions.TacticsGrid.GridTiles.TryGetValue(index, out TileData tileData);
                if (!tileData.unitOnTile)
                {
                    List<TileType> validTileTypes = DataManager.GetUnitDataFromType((UnitId)actionValue).unitStats.validTileTypes;

                    if (validTileTypes != null || validTileTypes.Count > 0 || validTileTypes.Contains(tileData.tileType))
                    {
                        Unit newUnit = Instantiate(_unitPrefab);
                        newUnit.gameObject.name = ((UnitId)actionValue).ToString();
                        newUnit.InitializeUnit((UnitId)actionValue);
                        newUnit.SetUnitsGrid(_playerActions.TacticsGrid);

                        CombatSystem.Instance.AddUnitToCombat(index, newUnit, _unitTeamIndex);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}