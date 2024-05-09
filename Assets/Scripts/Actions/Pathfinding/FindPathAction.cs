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
                _lastPath.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsPath));
                _lastPath.Clear();
            }

            if (index == _lastIndex)
            {
                _lastIndex = new GridIndex(int.MinValue, int.MinValue);
                return false;
            }
            _lastIndex = index;

            GridIndex previousTile = _playerActions.SelectedTile;
            if (previousTile != index)
            {
                PathData pathData;
                pathData.allowPartialSolution = true;
                pathData.heightAllowance = 2f;
                pathData.includeDiagonals = actionValue == 1;
                pathData.includeStartNode = false;

                PathResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(new GridIndex(0, 0), index, out List<GridIndex> path, pathData);
                if (result == PathResult.SearchSuccess)
                {
                    _lastPath = path;
                    for (int i = 0; i < path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(path[i], TileState.IsPath);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}