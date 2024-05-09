using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class FindPathAction : ActionBase
	{
        private List<GridIndex> _lastPath = new List<GridIndex>();
        public override bool ExecuteAction(GridIndex index)
        {
            if (_lastPath.Count > 0)
            {
                _lastPath.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsPath));
                _lastPath.Clear();
            }

            GridIndex previousTile = _playerActions.SelectedTile;
            if (previousTile != index)
            {
                PathResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(new GridIndex(0, 0), index, out List<GridIndex> path);
                if (result == PathResult.SearchSuccess)
                {
                    _lastPath = path;
                    for (int i = 0; i < path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(path[i], TileState.IsPath);
                    }
                }
            }
            return false;
        }
    }
}