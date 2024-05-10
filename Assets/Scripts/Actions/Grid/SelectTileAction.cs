using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
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
                _playerActions.SelectedTile = new GridIndex(int.MinValue, int.MinValue);
            }
            return false;
        }

        private void OnDestroy()
        {
            ExecuteAction(new GridIndex(int.MinValue, int.MinValue));
        }
    }
}
