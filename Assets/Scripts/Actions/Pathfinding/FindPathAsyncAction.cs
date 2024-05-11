using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class FindPathAsyncAction : ActionBase
    {
        private List<GridIndex> _lastPath = new List<GridIndex>();
        private GridIndex _lastIndex;
        private bool _isSearching = false;

        public override bool ExecuteAction(GridIndex index)
        {
            if (_lastPath.Count > 0)
            {
                _lastPath.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInPath));
                _lastPath.Clear();
                _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();
            }

            GridIndex previousTile = _playerActions.SelectedTile;
            if (previousTile != index)
            {
                if (!_isSearching)
                {
                    ExecuteActionAsync(index);
                }
            }
            return false;
        }
        private async void ExecuteActionAsync(GridIndex index)
        {
            PathfindingResult pathResult = await Task.Run(() => { return _playerActions.TacticsGrid.GridPathfinder.FindPath(_playerActions.SelectedTile, index); });

            _playerActions.TacticsGrid.GridPathfinder.OnPathfindingCompleted?.Invoke();

            _isSearching = false;

            if (pathResult.Result == PathResult.SearchSuccess || pathResult.Result == PathResult.GoalUnreachable)
            {
                _lastPath = pathResult.Path;
                for (int i = 0; i < pathResult.Path.Count; i++)
                {
                    _playerActions.TacticsGrid.AddStateToTile(pathResult.Path[i], TileState.IsInPath);
                }
            }
        }

        private void OnDestroy()
        {
            if (_lastPath.Count > 0)
            {
                _lastPath.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInPath));
            }
            _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();
        }
    }
}