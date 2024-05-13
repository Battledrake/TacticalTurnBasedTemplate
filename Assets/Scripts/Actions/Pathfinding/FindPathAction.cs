using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class FindPathAction : ActionBase
    {
        private List<GridIndex> _lastPath = new List<GridIndex>();

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
                PathfindingResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(_playerActions.SelectedTile, index);

                _playerActions.TacticsGrid.GridPathfinder.OnPathfindingCompleted?.Invoke();

                if (result.Result == PathResult.SearchSuccess || result.Result == PathResult.GoalUnreachable)
                {
                    _lastPath = result.Path;
                    for (int i = 0; i < result.Path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(result.Path[i], TileState.IsInPath);
                    }
                    return true;
                }
            }
            return false;
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