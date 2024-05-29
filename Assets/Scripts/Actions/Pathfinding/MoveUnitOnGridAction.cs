using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MoveUnitOnGridAction : ActionBase
    {
        private Unit _currentUnit;

        public override bool ExecuteAction(GridIndex index)
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);

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
                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_currentUnit);

                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindPath(_currentUnit.UnitGridIndex, index, pathParams);
                if (pathResult.Result == PathResult.SearchSuccess)
                {
                    for (int i = 0; i < pathResult.Path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(pathResult.Path[i], TileState.IsInPath);
                    }
                    CombatManager.Instance.MoveUnit(_currentUnit, pathResult.Path);

                    _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
                    _currentUnit.OnUnitReachedDestination += SelectedUnit_OnUnitReachedDestination;
                    return true;
                }
            }
            return false;
        }

        private void SelectedUnit_OnUnitReachedDestination(Unit unit)
        {
            unit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
        }

        private void OnDestroy()
        {
            if (_currentUnit != null)
                _currentUnit.OnUnitReachedDestination -= SelectedUnit_OnUnitReachedDestination;

            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
        }
    }
}