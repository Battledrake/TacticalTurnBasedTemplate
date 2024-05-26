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
                _playerActions.TacticsGrid.ClearStateFromTiles(_previousPath,TileState.IsInPath);
                _previousPath.Clear();
            }

            if (_currentUnit != null)
            {
                if (_currentUnit != _playerActions.SelectedUnit)
                {
                    _currentUnit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;
                }
                else
                {
                    if (_currentUnit.IsMoving)
                        return false;
                }
            }

            _currentUnit = _playerActions.SelectedUnit;
            if (_currentUnit != null)
            {

                PathFilter pathFilter = GridPathfinding.CreatePathFilterFromUnit(_currentUnit, false, false);

                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindPath(_currentUnit.UnitGridIndex, index, pathFilter);
                if (pathResult.Result == PathResult.SearchSuccess)
                {
                    for (int i = 0; i < pathResult.Path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(pathResult.Path[i], TileState.IsInPath);
                    }
                    _previousPath = new List<GridIndex>(pathResult.Path);
                    _currentUnit.GetComponent<GridMovement>().SetPathAndMove(pathResult.Path);

                    _playerActions.TacticsGrid.ClearStateFromTiles(_previousPath, TileState.IsInMoveRange);
                    _currentUnit.OnUnitReachedDestination += SelectedUnit_OnUnitReachedDestination;
                    return true;
                }
            }
            return false;
        }

        private void SelectedUnit_OnUnitReachedDestination(Unit unit)
        {
            unit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;
            _playerActions.TacticsGrid.ClearStateFromTiles(_previousPath, TileState.IsInPath);
        }

        private void OnDestroy()
        {
            if (_currentUnit != null)
                _currentUnit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;

            _playerActions.TacticsGrid.ClearStateFromTiles(_previousPath,TileState.IsInPath);
            _previousPath.Clear();
        }
    }
}