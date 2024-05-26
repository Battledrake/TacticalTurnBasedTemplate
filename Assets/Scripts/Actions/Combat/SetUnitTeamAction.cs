using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SetUnitTeamAction : ActionBase
	{
        public int UnitTeamIndex { get => _unitTeamIndex; set => _unitTeamIndex = value; }

        private int _unitTeamIndex;

        public override bool ExecuteAction(GridIndex index)
        {
            if(_playerActions.TacticsGrid.GetTileDataFromIndex(index, out TileData selectedTileData))
            {
                if (selectedTileData.unitOnTile)
                {
                    CombatSystem.Instance.SetUnitTeamIndex(selectedTileData.unitOnTile, _unitTeamIndex);
                }
            }
            return false;
        }
    }
}