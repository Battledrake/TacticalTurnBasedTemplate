using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class FindPathAsyncAction : ActionBase
    {
        private bool _isSearching = false;

        public override bool ExecuteAction(GridIndex index)
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);

            GridIndex previousTile = _playerActions.SelectedTile;
            if (previousTile != index)
            {
                if (!_isSearching)
                {
                    ExecuteActionAsync(index);
                    _isSearching = true;
                }
            }
            return false;
        }
        private async void ExecuteActionAsync(GridIndex index)
        {
            PathParams filter = _playerActions.TacticsGrid.Pathfinder.CreateDefaultPathParams(Mathf.Infinity);

            PathfindingResult pathResult = await Task.Run(() => { return _playerActions.TacticsGrid.Pathfinder.FindPath(_playerActions.SelectedTile, index, filter); });

            _playerActions.TacticsGrid.Pathfinder.OnPathfindingCompleted?.Invoke();

            if (pathResult.Result != PathResult.SearchFail)
            {
                List<GridIndex> pathIndexes = PathfindingStatics.ConvertPathNodesToGridIndexes(pathResult.Path);
                for (int i = 0; i < pathResult.Path.Count; i++)
                {
                    _playerActions.TacticsGrid.AddStateToTile(pathIndexes[i], TileState.IsInPath);
                }
            }

            _isSearching = false;
        }

        private void OnDestroy()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
        }
    }
}