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
                    Unit newUnit = Instantiate(_unitPrefab);
                    newUnit.InitializeUnit((UnitType)actionValue);

                    _playerActions.CombatSystem.AddUnitInCombat(newUnit, index);
                    return true;
                }
            }
            return false;
        }
    }
}