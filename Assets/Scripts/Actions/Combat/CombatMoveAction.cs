using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatMoveAction : ActionBase
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Color _moveRangeColor;
        [SerializeField] private Color _sprintRangeColor;
        [SerializeField] private Color _outOfRangeColor;

        private Unit _currentUnit;
        private List<GridIndex> _generatedPath = new List<GridIndex>();
        private List<GridIndex> _moveRangeIndexes = new List<GridIndex>();
        private bool _isUnitMoving = false;

        private void Start()
        {
            if (UnitHasEnoughAbilityPoints())
            {
                GenerateTilesInMoveRange(_playerActions.SelectedUnit);
                GeneratePathForUnit();
            }
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
            if (_isUnitMoving) return;

            if (!_playerActions.SelectedUnit) return;

            if (!UnitHasEnoughAbilityPoints()) return;


            if (_playerActions.HoveredUnit && CombatManager.Instance.ShowEnemyMoveRange)
            {
                GenerateTilesInMoveRange(_playerActions.HoveredUnit);
            }
            else
            {
                GeneratePathForUnit();
            }
        }

        private void GenerateTilesInMoveRange(Unit unit)
        {
            if (_currentUnit != unit)
            {
                _currentUnit = unit;

                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);

                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(unit, unit.MoveRange);
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(unit.UnitGridIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    _moveRangeIndexes = pathResult.Path;
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

            if (!UnitHasEnoughAbilityPoints())
                return;

            _generatedPath.Clear();
            int unitMoveRange = _playerActions.SelectedUnit.MoveRange;
            float pathLength = UnitHasEnoughAbilityPoints(2) ? unitMoveRange * 2 : unitMoveRange;

            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_playerActions.SelectedUnit, pathLength, true);
            PathfindingResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(_playerActions.SelectedTile, _playerActions.HoveredTile, pathParams);

            if (result.Result == PathResult.SearchSuccess || result.Result == PathResult.GoalUnreachable)
            {
                _generatedPath = result.Path;


                EnableAndInitializePathLine(result.Path.Count);

                for (int i = 0; i < result.Path.Count; i++)
                {
                    _playerActions.TacticsGrid.GetTileDataFromIndex(result.Path[i], out TileData tileData);
                    _lineRenderer.SetPosition(i, tileData.tileMatrix.GetPosition() + new Vector3(0f, 0.5f, 0f));
                }
            }
        }

        private bool UnitHasEnoughAbilityPoints(int amountNeeded = 1)
        {
            if (_playerActions.SelectedUnit)
            {
                AbilitySystem abilitySystem = _playerActions.SelectedUnit.GetComponent<IAbilitySystem>().GetAbilitySystem();
                if (abilitySystem)
                {
                    return abilitySystem.CurrentAbilityPoints >= amountNeeded;
                }
            }
            return false;
        }

        private void Unit_OnUnitStartedMovement(Unit unit)
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
            unit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;

            _currentUnit = null;
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _isUnitMoving = false;
            _lineRenderer.enabled = false;

            unit.OnUnitStartedMovement -= Unit_OnUnitReachedDestination;

            if (UnitHasEnoughAbilityPoints())
                GenerateTilesInMoveRange(unit);
        }

        private void EnableAndInitializePathLine(int count)
        {
            _lineRenderer.positionCount = count;
            _lineRenderer.enabled = true;
            _lineRenderer.startColor = _moveRangeIndexes.Contains(_playerActions.HoveredTile) ? _moveRangeColor : _sprintRangeColor;
            Color endColor = _lineRenderer.startColor;
            endColor.a = 0.1f;
            _lineRenderer.endColor = endColor;
            _lineRenderer.startWidth = 0.2f;
            _lineRenderer.endWidth = 0.15f;
        }

        private void OnDisable()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
        }
    }
}