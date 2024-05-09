using UnityEngine;

namespace BattleDrakeCreations.TTBTk
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