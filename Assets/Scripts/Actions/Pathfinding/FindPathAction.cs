using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class FindPathAction : ActionBase
	{
        private List<GridIndex> _lastPath = new List<GridIndex>();
        private GridIndex _lastIndex;

        public override bool ExecuteAction(GridIndex index)
        {
            if (_lastPath.Count > 0)
            {
                _lastPath.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInPath));
                _lastPath.Clear();
            }

            GridIndex previousTile = _playerActions.SelectedTile;
            if (previousTile != index)
            {
                PathResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(_playerActions.SelectedTile, index, out List<GridIndex> path);
                if (result == PathResult.SearchSuccess || result == PathResult.GoalUnreachable)
                {
                    _lastPath = path;
                    for (int i = 0; i < path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(path[i], TileState.IsInPath);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}