using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowMoveRangeAction : SelectTileAndUnitAction
    {
        private List<GridIndex> _currentTilesInRange = new List<GridIndex>();
        private Unit _currentUnit;

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_currentTilesInRange.Count > 0)
            {
                _playerActions.TacticsGrid.ClearStateFromTiles(_currentTilesInRange, TileState.IsInMoveRange);
                _currentTilesInRange.Clear();
            }

            if (_currentUnit != null && _currentUnit != _playerActions.SelectedUnit)
            {
                _currentUnit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;
                _currentUnit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;
            }


            if (index != _playerActions.SelectedTile)
                return false;

            _currentUnit = _playerActions.SelectedUnit;

            PathParams pathParams;

            if (_currentUnit != null)
            {
                pathParams = GridPathfinding.CreatePathParamsFromUnit(_currentUnit, _currentUnit.GetMoveRange());
            }
            else
            {
                pathParams = _playerActions.TacticsGrid.GridPathfinder.CreateDefaultPathParams(actionValue);
            }

            if (GenerateTilesInRange(index, pathParams))
            {
                return true;
            }
            return false;
        }

        private bool GenerateTilesInRange(GridIndex index, PathParams filter)
        {
            PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(index, filter);
            if (pathResult.Result != PathResult.SearchFail)
            {
                _currentTilesInRange = pathResult.Path;
                for(int i = 0; i < _currentTilesInRange.Count; i++)
                {
                    _playerActions.TacticsGrid.AddStateToTile(_currentTilesInRange[i], TileState.IsInMoveRange);
                }
                if (_currentUnit)
                    _currentUnit.OnUnitStartedMovement += Unit_OnUnitStartedMovement;

                return true;
            }
            return false;
        }

        private void Unit_OnUnitStartedMovement(Unit unit)
        {
            _playerActions.TacticsGrid.ClearStateFromTiles(_currentTilesInRange, TileState.IsInMoveRange);
            if (_currentUnit == unit)
            {
                unit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;
                unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
            }

        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _currentUnit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;

            if (unit == _currentUnit)
            {
                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(unit, unit.GetMoveRange());
                GenerateTilesInRange(unit.UnitGridIndex, pathParams);
            }
        }

        private void OnDestroy()
        {
            _playerActions.TacticsGrid.ClearStateFromTiles(_currentTilesInRange, TileState.IsInMoveRange);
            _currentTilesInRange.Clear();

            if (_currentUnit != null)
            {
                _currentUnit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;
                _currentUnit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;
            }

        }
    }
}