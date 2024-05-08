using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class SelectTileAction : ActionBase
    {
        public override bool ExecuteAction(Vector2Int index)
        {
            Vector2Int previousTile = _playerActions.SelectedTile;
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
                _playerActions.SelectedTile = new Vector2Int(-999, -999);
            }
            return false;
        }

        private void OnDestroy()
        {
            ExecuteAction(new Vector2Int(-999, -999));
        }
    }
}
