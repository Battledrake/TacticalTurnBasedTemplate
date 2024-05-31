using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatMoveAction : ActionBase
    {

        private Unit _currentUnit;
        private List<GridIndex> _generatedPath = new List<GridIndex>();
        private bool _isUnitMoving = false;

        private void Start()
        {
            GenerateTilesInMoveRange(_playerActions.SelectedUnit);
            GeneratePathForUnit();
        }

        public override bool ExecuteAction(GridIndex index)
        {
            if (_generatedPath.Count > 0)
            {
                _playerActions.SelectedUnit.OnUnitStartedMovement += Unit_OnUnitStartedMovement;
                _playerActions.SelectedUnit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;

                CombatManager.Instance.MoveUnit(_playerActions.SelectedUnit, _generatedPath);

                _generatedPath.Clear();
                _isUnitMoving = true;

                return true;
            }
            return false;
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            if (_isUnitMoving)
                return;

            if (!_playerActions.SelectedUnit)
                return;

            if (_playerActions.SelectedUnit.CurrentActionPoints <= 0)
                return;

            if (_playerActions.HoveredUnit && CombatManager.Instance.ShowEnemyMoveRange)
            {
                GenerateTilesInMoveRange(_playerActions.HoveredUnit);
            }
            else
            {
                GenerateTilesInMoveRange(_playerActions.SelectedUnit);
                GeneratePathForUnit();
            }
        }

        private void GenerateTilesInMoveRange(Unit unit)
        {
            if (_currentUnit != unit)
            {
                _currentUnit = unit;

                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);

                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(unit);
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(unit.UnitGridIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    for (int i = 0; i < pathResult.Path.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(pathResult.Path[i], TileState.IsInMoveRange);
                    }
                }
            }
        }

        private void GeneratePathForUnit()
        {
            if (!_playerActions.SelectedUnit)
                return;

            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
            _generatedPath.Clear();

            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_playerActions.SelectedUnit);
            PathfindingResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(_playerActions.SelectedTile, _playerActions.HoveredTile, pathParams);

            if (result.Result == PathResult.SearchSuccess || result.Result == PathResult.GoalUnreachable)
            {
                //What's this for again?
                //_playerActions.TacticsGrid.GridPathfinder.OnPathfindingCompleted?.Invoke();
                _generatedPath = result.Path;

                for (int i = 0; i < result.Path.Count; i++)
                {
                    _playerActions.TacticsGrid.AddStateToTile(_generatedPath[i], TileState.IsInPath);
                }
            }
        }

        private void Unit_OnUnitStartedMovement(Unit unit)
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
            unit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;

            _currentUnit = null;
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _isUnitMoving = false;

            unit.OnUnitStartedMovement -= Unit_OnUnitReachedDestination;

            if (unit.CurrentActionPoints > 0)
                GenerateTilesInMoveRange(unit);
        }

        private void OnDisable()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
        }

        private void OnDestroy()
        {
        }
    }
}