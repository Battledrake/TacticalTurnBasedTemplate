using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class SelectTileAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            GridIndex previousTile = _playerActions.SelectedTile;
            if(previousTile != index)
            {
                _playerActions.TacticsGrid.RemoveStateFromTile(previousTile, TileState.Selected);
                _playerActions.SelectedTile = index;
                _playerActions.TacticsGrid.AddStateToTile(index, TileState.Selected);

                return true;
            }
            else //Clicked on a tile that was already selected
            {
                _playerActions.TacticsGrid.RemoveStateFromTile(index, TileState.Selected);
                _playerActions.SelectedTile = new GridIndex(-999, -999);
            }
            return false;
        }

        private void OnDestroy()
        {
            ExecuteAction(new GridIndex(-999, -999));
        }
    }
}
