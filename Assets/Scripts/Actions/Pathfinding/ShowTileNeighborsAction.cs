using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class ShowTileNeighborsAction : ActionBase
    {
        public override bool ExecuteAction(Vector2Int index)
        {
            _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsNeighbor);

            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                List<Vector2Int> neighbors = _playerActions.TacticsGrid.GridPathfinder.GetValidTileNeighbors(index);
                neighbors.ForEach(n =>
                {
                    _playerActions.TacticsGrid.AddStateToTile(n, TileState.IsNeighbor);
                });
                return true;
            }
            return false;
        }
    }
}