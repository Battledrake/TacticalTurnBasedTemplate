using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class ShowTileNeighborsAction : ActionBase
	{
        public override bool ExecuteAction(Vector2Int index)
        {
            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                _playerActions.TacticsGrid.AddStateToTile(index, TileState.IsNeighbor);
            }
            return false;
        }
    }
}