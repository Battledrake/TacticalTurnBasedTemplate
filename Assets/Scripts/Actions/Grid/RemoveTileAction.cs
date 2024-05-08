using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class RemoveTileAction : ActionBase
	{
        public override bool ExecuteAction(Vector2Int index)
        {
            _playerActions.TacticsGrid.RemoveGridTile(index);

            return true;
        }
    }
}