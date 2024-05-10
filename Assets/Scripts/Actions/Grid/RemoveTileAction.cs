using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class RemoveTileAction : ActionBase
	{
        public override bool ExecuteAction(GridIndex index)
        {
            _playerActions.TacticsGrid.RemoveGridTile(index);

            return true;
        }
    }
}