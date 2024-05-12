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

            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                Unit newUnit = Instantiate(_unitPrefab, _playerActions.TacticsGrid.GetTilePositionFromGridIndex(index), Quaternion.identity);
                newUnit.InitializeUnit((UnitType)actionValue);
                return true;
            }
            return false;
        }
    }
}