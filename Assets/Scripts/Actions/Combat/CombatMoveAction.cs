using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatMoveAction : ActionBase
	{
        private List<GridIndex> _currentTilesInRange = new List<GridIndex>();

        private Unit _currentUnit;

        private void Start()
        {
            GenerateTilesInMoveRange(_playerActions.SelectedUnit);
        }

        public override bool ExecuteAction(GridIndex index)
        {
            //Display Reachables
            //Display Path while active.
            //Click tile = move
            //Hover unit, display range.
            //Don't display reachables on movement
            //Hide path when start movement
            return true;
        }

        private void GenerateTilesInMoveRange(Unit unit)
        {
            if(_currentUnit != unit)
            {
                _currentUnit = unit;

                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);

                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(unit);
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(unit.UnitGridIndex, pathParams);
                if(pathResult.Result != PathResult.SearchFail)
                {
                    _currentTilesInRange = pathResult.Path;
                    for (int i = 0; i < _currentTilesInRange.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(_currentTilesInRange[i], TileState.IsInMoveRange);
                    }
                }
            }
        }

        private void Unit_OnUnitStartedMovement(Unit unit)
        {
            _playerActions.TacticsGrid.ClearStateFromTiles(_currentTilesInRange, TileState.IsInMoveRange);
            if (_currentUnit == unit)
            {
                unit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;
            }
            //Hide Reachables
        }
    }
}