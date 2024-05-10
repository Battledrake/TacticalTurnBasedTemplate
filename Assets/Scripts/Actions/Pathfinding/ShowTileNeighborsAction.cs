using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowTileNeighborsAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsNeighbor);

            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                bool includeDiagonals = actionValue == 1;
                List<GridIndex> neighbors = _playerActions.TacticsGrid.GridPathfinder.GetValidTileNeighbors(index, includeDiagonals);
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