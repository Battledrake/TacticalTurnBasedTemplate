using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatMoveAction : ActionBase
    {
        [SerializeField] private LineRenderer _pathLine;
        [SerializeField] private Color _moveRangeColor;
        [SerializeField] private Color _sprintRangeColor;
        [SerializeField] private Color _outOfRangeColor;
        [Range(0.1f, 1f)]
        [SerializeField] private float _outlineLength = 1f;
        [Range(0.1f, 1f)]
        [SerializeField] private float _distanceToEdge = 1f;

        private Unit _currentUnit;

        private List<GridIndex> _generatedPath = new List<GridIndex>();

        private List<GridIndex> _moveRangeIndexes = new List<GridIndex>();
        private List<EdgeData> _moveRangeEdges = new List<EdgeData>();

        private List<LineRenderer> _borrowedMoveRenders = new List<LineRenderer>();
        private List<LineRenderer> _borrowedSprintRenders = new List<LineRenderer>();

        private bool _isUnitMoving = false;
        private bool _isSprintRangeShowing = false;

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
                ShowSprintRangeTiles();

                GeneratePathForUnit();
            }
        }

        private void GenerateTilesInMoveRange(Unit unit)
        {
            if (_currentUnit != unit)
            {
                _currentUnit = unit;

                LineRendererPool.Instance.ReturnInstances(_borrowedMoveRenders);
                LineRendererPool.Instance.ReturnInstances(_borrowedSprintRenders);


                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(unit, unit.MoveRange);
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(unit.UnitGridIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    _moveRangeIndexes = pathResult.Path;
                    _moveRangeEdges = pathResult.Edges;

                    _borrowedMoveRenders = LineRendererPool.Instance.BorrowInstances(_moveRangeEdges.Count);

                    for (int i = 0; i < _moveRangeEdges.Count; i++)
                    {
                        _borrowedMoveRenders[i].positionCount = 2;
                        _borrowedMoveRenders[i].startColor = _moveRangeColor;
                        _borrowedMoveRenders[i].endColor = _moveRangeColor;
                        _borrowedMoveRenders[i].startWidth = 0.15f;
                        _borrowedMoveRenders[i].endWidth = 0.15f;

                        Vector3 sourcePosition = _playerActions.TacticsGrid.GetTilePositionFromIndex(_moveRangeEdges[i].source);
                        GridIndex direction = _moveRangeEdges[i].direction;
                        _borrowedMoveRenders[i].SetPosition(0, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) - ((float)direction.z * _outlineLength), sourcePosition.y + 0.5f, sourcePosition.z + (direction.z * _distanceToEdge) - ((float)direction.x * _outlineLength)));
                        _borrowedMoveRenders[i].SetPosition(1, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) + ((float)direction.z * _outlineLength), sourcePosition.y + 0.5f, sourcePosition.z + (direction.z * _distanceToEdge) + ((float)direction.x * _outlineLength)));
                    }

                    ShowSprintRangeTiles();
                }
            }
        }

        private void ShowSprintRangeTiles()
        {
            if (!UnitHasEnoughAbilityPoints(2) || _moveRangeIndexes.Contains(_playerActions.HoveredTile))
            {
                if (!_isSprintRangeShowing)
                    return;

                for(int i = 0; i < _borrowedSprintRenders.Count; i++)
                {
                    _borrowedSprintRenders[i].gameObject.SetActive(false);
                }
                _isSprintRangeShowing = false;
                return;
            }

            if (_borrowedSprintRenders.Count > 0)
            {
                if (_isSprintRangeShowing)
                    return;

                for(int i = 0; i < _borrowedSprintRenders.Count; i++)
                {
                    _borrowedSprintRenders[i].gameObject.SetActive(true);
                }
                _isSprintRangeShowing = true;
                return;
            }

            GenerateSprintRangeTiles();
        }

        private void GenerateSprintRangeTiles()
        {
            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_playerActions.SelectedUnit, _playerActions.SelectedUnit.MoveRange * 2);
            PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(_playerActions.SelectedTile, pathParams);
            if (pathResult.Result != PathResult.SearchFail)
            {
                List<EdgeData> sprintEdges = new List<EdgeData>(pathResult.Edges);
                for (int i = 0; i < _moveRangeEdges.Count; i++)
                {
                    if (pathResult.Edges.Contains(_moveRangeEdges[i]))
                    {
                        sprintEdges.Remove(_moveRangeEdges[i]);
                    }
                }

                _borrowedSprintRenders = LineRendererPool.Instance.BorrowInstances(sprintEdges.Count);

                for (int i = 0; i < sprintEdges.Count; i++)
                {
                    _borrowedSprintRenders[i].positionCount = 2;
                    _borrowedSprintRenders[i].startColor = _sprintRangeColor;
                    _borrowedSprintRenders[i].endColor = _sprintRangeColor;
                    _borrowedSprintRenders[i].startWidth = 0.15f;
                    _borrowedSprintRenders[i].endWidth = 0.15f;

                    Vector3 sourcePosition = _playerActions.TacticsGrid.GetTilePositionFromIndex(sprintEdges[i].source);
                    GridIndex direction = sprintEdges[i].direction;
                    _borrowedSprintRenders[i].SetPosition(0, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) - ((float)direction.z * _outlineLength), sourcePosition.y + 0.5f, sourcePosition.z + (direction.z * _distanceToEdge) - ((float)direction.x * _outlineLength)));
                    _borrowedSprintRenders[i].SetPosition(1, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) + ((float)direction.z * _outlineLength), sourcePosition.y + 0.5f, sourcePosition.z + (direction.z * _distanceToEdge) + ((float)direction.x * _outlineLength)));
                }
                _isSprintRangeShowing = true;
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

                List<Vector3> pathPositions = new List<Vector3>();

                for (int i = 0; i < result.Path.Count; i++)
                {
                    _pathLine.SetPosition(i, _playerActions.TacticsGrid.GetTilePositionFromIndex(result.Path[i]) + new Vector3(0f, 0.5f, 0f));
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
            LineRendererPool.Instance.ReturnInstances(_borrowedMoveRenders);
            LineRendererPool.Instance.ReturnInstances(_borrowedSprintRenders);

            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
            unit.OnUnitStartedMovement -= Unit_OnUnitStartedMovement;

            _currentUnit = null;
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _isUnitMoving = false;
            if (_pathLine != null)
                _pathLine.enabled = false;

            unit.OnUnitStartedMovement -= Unit_OnUnitReachedDestination;

            if (UnitHasEnoughAbilityPoints())
                GenerateTilesInMoveRange(unit);
        }

        private void EnableAndInitializePathLine(int count)
        {
            _pathLine.positionCount = count;
            _pathLine.enabled = true;
            _pathLine.startColor = _moveRangeIndexes.Contains(_playerActions.HoveredTile) ? _moveRangeColor : _sprintRangeColor;
            Color endColor = _pathLine.startColor;
            endColor.a = 0.1f;
            _pathLine.endColor = endColor;
            _pathLine.startWidth = 0.15f;
            _pathLine.endWidth = 0.15f;
        }

        private void OnDisable()
        {
            LineRendererPool.Instance.ReturnInstances(_borrowedMoveRenders);
            LineRendererPool.Instance.ReturnInstances(_borrowedSprintRenders);

            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
        }
    }
}