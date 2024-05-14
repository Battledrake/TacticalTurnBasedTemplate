using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MoveUnitOnGridAction : ActionBase
    {
        private List<GridIndex> _previousPath = new List<GridIndex>();
        private Unit _currentUnit;

        public override bool ExecuteAction(GridIndex index)
        {
            if (_previousPath.Count > 0)
            {
                _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsInPath);
                _previousPath.Clear();
            }

            if(_currentUnit != null && _currentUnit != _playerActions.SelectedUnit)
            {
                _currentUnit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;
            }

            _currentUnit = _playerActions.SelectedUnit;
            if (_currentUnit != null)
            {
                _currentUnit.OnUnitReachedDestination += SelectedUnit_OnUnitReachedDestination;

                PathData pathData;
                pathData.includeDiagonals = _currentUnit.UnitData.unitStats.canMoveDiagonal;
                pathData.heightAllowance = _currentUnit.UnitData.unitStats.heightAllowance;
                pathData.includeStartNode = false;
                pathData.allowPartialSolution = true;
                pathData.validTileTypes = _currentUnit.UnitData.unitStats.validTileTypes;

                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindPath(_currentUnit.UnitGridIndex, index, pathData);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    for(int i = 0; i < pathResult.Path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(pathResult.Path[i], TileState.IsInPath);
                    }
                    _currentUnit.SetPathAndMove(pathResult.Path);
                    _previousPath = pathResult.Path;
                    return true;
                }
            }
            return false;
        }

        private void SelectedUnit_OnUnitReachedDestination(Unit unit)
        {
            unit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;
            _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsInPath);
        }

        private void OnDestroy()
        {
            _previousPath.Clear();
            _playerActions.TacticsGrid.ClearStateFromTiles(TileState.IsInPath);
        }
    }
}