using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowSelectedMoveRangeAction : SelectTileAndUnitAction
    {
        private List<GridIndex> _currentTilesInRange = new List<GridIndex>();
        private Unit _currentUnit;

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_currentTilesInRange.Count > 0)
            {
                _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsInMoveRange);
                _currentTilesInRange.Clear();
            }

            if (index != _playerActions.SelectedTile)
                return false;

            PathFilter pathFilter;

            _currentUnit = _playerActions.SelectedUnit;
            if (_currentUnit != null)
            {
                pathFilter.includeDiagonals = _currentUnit.UnitData.unitStats.canMoveDiagonal;
                pathFilter.heightAllowance = _currentUnit.UnitData.unitStats.heightAllowance;
                pathFilter.includeStartNode = false;
                pathFilter.allowPartialSolution = true;
                pathFilter.validTileTypes = _currentUnit.UnitData.unitStats.validTileTypes;
                pathFilter.maxPathLength = _currentUnit.UnitData.unitStats.moveRange;
            }
            else
            {
                GridPathfinding pathfinder = _playerActions.TacticsGrid.GridPathfinder;
                pathFilter.includeDiagonals = pathfinder.IncludeDiagonals;
                pathFilter.heightAllowance = pathfinder.HeightAllowance;
                pathFilter.includeStartNode = pathfinder.IncludeStartNodeInPath;
                pathFilter.allowPartialSolution = pathfinder.AllowPartialSolution;
                pathFilter.validTileTypes = new List<TileType> { TileType.Normal, TileType.DoubleCost, TileType.TripleCost, TileType.FlyingOnly};
                pathFilter.maxPathLength = actionValue;
            }
            PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindPath(index, new GridIndex(-999, -999), pathFilter);
            if(pathResult.Result != PathResult.SearchFail)
            {
                _currentTilesInRange = new List<GridIndex>(_playerActions.TacticsGrid.GridPathfinder.ReachableList);
                foreach (var node in _currentTilesInRange)
                {
                    _playerActions.TacticsGrid.AddStateToTile(node, TileState.IsInMoveRange);
                }
                return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            _currentTilesInRange.Clear();
            _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsInMoveRange);
        }
    }
}