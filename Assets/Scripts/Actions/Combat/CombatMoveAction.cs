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
        [Range(0.1f, 5f)]
        [SerializeField] private float _outlineHeight = 0.5f;
        [Range(0.1f, 1f)]
        [SerializeField] private float _pathLineSize = 0.2f;
        [Range(0, 10f)]
        [SerializeField] private float _pathLineSpeed = 3.5f;

        private Unit _currentUnit;

        private List<GridIndex> _generatedPath = new List<GridIndex>();
        private float _generatedPathLength = 0f;

        private List<GridIndex> _moveRangeIndexes = new List<GridIndex>();
        private HashSet<EdgeData> _moveRangeEdges = new HashSet<EdgeData>();

        private List<LineRenderer> _borrowedMoveRenders = new List<LineRenderer>();
        private List<LineRenderer> _borrowedSprintRenders = new List<LineRenderer>();

        private bool _isUnitMoving = false;
        private bool _isSprintRangeShowing = false;

        private void Start()
        {
            if (UnitHasEnoughActionPoints())
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

                CombatManager.Instance.MoveUnit(_playerActions.SelectedUnit, _generatedPath, _generatedPathLength);

                _generatedPath.Clear();
                _generatedPathLength = 0f;
                _isUnitMoving = true;

                return true;
            }
            return false;
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            if (_isUnitMoving) return;

            if (!_playerActions.SelectedUnit) return;

            if (!UnitHasEnoughActionPoints()) return;


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


                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_currentUnit, unit.MoveRange);
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(_currentUnit.UnitGridIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    _moveRangeIndexes = pathResult.Path;
                    _moveRangeEdges = pathResult.Edges;

                    _borrowedMoveRenders = LineRendererPool.Instance.BorrowInstances(_moveRangeEdges.Count);
                    int edgeIndex = 0;

                    foreach(var edgePair in _moveRangeEdges)
                    {
                        _borrowedMoveRenders[edgeIndex].positionCount = 2;
                        _borrowedMoveRenders[edgeIndex].startColor = UnitHasEnoughActionPoints(2) ? _moveRangeColor : _sprintRangeColor;
                        _borrowedMoveRenders[edgeIndex].endColor = UnitHasEnoughActionPoints(2) ? _moveRangeColor : _sprintRangeColor; ;
                        _borrowedMoveRenders[edgeIndex].startWidth = 0.15f;
                        _borrowedMoveRenders[edgeIndex].endWidth = 0.15f;

                        Vector3 sourcePosition = _playerActions.TacticsGrid.GetTilePositionFromIndex(edgePair.source);
                        GridIndex direction = edgePair.direction;
                        _borrowedMoveRenders[edgeIndex].SetPosition(0, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) - ((float)direction.z * _outlineLength), sourcePosition.y + _outlineHeight, sourcePosition.z + (direction.z * _distanceToEdge) - ((float)direction.x * _outlineLength)));
                        _borrowedMoveRenders[edgeIndex].SetPosition(1, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) + ((float)direction.z * _outlineLength), sourcePosition.y + _outlineHeight, sourcePosition.z + (direction.z * _distanceToEdge) + ((float)direction.x * _outlineLength)));
                        edgeIndex++;
                    }

                    ShowSprintRangeTiles();
                }
            }
        }

        private void Update()
        {
            _pathLine.material.mainTextureOffset = new Vector2(Time.time * -_pathLineSpeed, 0);
        }

        private void ShowSprintRangeTiles()
        {
            if (!UnitHasEnoughActionPoints(2) || _moveRangeIndexes.Contains(_playerActions.HoveredTile))
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
            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_currentUnit, _currentUnit.MoveRange * 2);
            PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(_currentUnit.UnitGridIndex, pathParams);
            if (pathResult.Result != PathResult.SearchFail)
            {
                HashSet<EdgeData> sprintEdges = new HashSet<EdgeData>(pathResult.Edges);
                foreach(var edgePair in _moveRangeEdges)
                {
                    if (pathResult.Edges.Contains(edgePair))
                    {
                        sprintEdges.Remove(edgePair);
                    }
                }

                _borrowedSprintRenders = LineRendererPool.Instance.BorrowInstances(sprintEdges.Count);
                int edgeIndex = 0;
                foreach(var edgePair in sprintEdges)
                {
                    _borrowedSprintRenders[edgeIndex].positionCount = 2;
                    _borrowedSprintRenders[edgeIndex].startColor = _sprintRangeColor;
                    _borrowedSprintRenders[edgeIndex].endColor = _sprintRangeColor;
                    _borrowedSprintRenders[edgeIndex].startWidth = 0.15f;
                    _borrowedSprintRenders[edgeIndex].endWidth = 0.15f;

                    Vector3 sourcePosition = _playerActions.TacticsGrid.GetTilePositionFromIndex(edgePair.source);
                    GridIndex direction = edgePair.direction;
                    _borrowedSprintRenders[edgeIndex].SetPosition(0, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) - ((float)direction.z * _outlineLength), sourcePosition.y + _outlineHeight, sourcePosition.z + (direction.z * _distanceToEdge) - ((float)direction.x * _outlineLength)));
                    _borrowedSprintRenders[edgeIndex].SetPosition(1, new Vector3(sourcePosition.x + (direction.x * _distanceToEdge) + ((float)direction.z * _outlineLength), sourcePosition.y + _outlineHeight, sourcePosition.z + (direction.z * _distanceToEdge) + ((float)direction.x * _outlineLength)));
                    edgeIndex++;
                }
                _isSprintRangeShowing = true;
            }
        }

        private void GeneratePathForUnit()
        {
            if (!_playerActions.SelectedUnit)
                return;

            if (!UnitHasEnoughActionPoints())
                return;

            _generatedPath.Clear();
            int unitMoveRange = _playerActions.SelectedUnit.MoveRange;
            float pathLength = UnitHasEnoughActionPoints(2) ? unitMoveRange * 2 : unitMoveRange;

            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_playerActions.SelectedUnit, pathLength, true);
            PathfindingResult result = _playerActions.TacticsGrid.GridPathfinder.FindPath(_playerActions.SelectedTile, _playerActions.HoveredTile, pathParams);

            if (result.Result == PathResult.SearchSuccess || result.Result == PathResult.GoalUnreachable)
            {
                _generatedPath = result.Path;
                _generatedPathLength = result.Length;

                EnableAndInitializePathLine(result.Path.Count);

                List<Vector3> pathPositions = new List<Vector3>();

                for (int i = 0; i < result.Path.Count; i++)
                {
                    _pathLine.SetPosition(i, _playerActions.TacticsGrid.GetTilePositionFromIndex(result.Path[i]) + new Vector3(0f, 0.5f, 0f));
                }
            }
        }

        private bool UnitHasEnoughActionPoints(int amountNeeded = 1)
        {
            if (_playerActions.SelectedUnit)
            {
                AbilitySystem abilitySystem = _playerActions.SelectedUnit.GetComponent<IAbilitySystem>().GetAbilitySystem();
                if (abilitySystem)
                {
                    return abilitySystem.CurrentActionPoints >= amountNeeded;
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

            if (UnitHasEnoughActionPoints())
                GenerateTilesInMoveRange(unit);
        }

        private void EnableAndInitializePathLine(int count)
        {
            _pathLine.positionCount = count;
            _pathLine.enabled = true;
            _pathLine.startColor = _moveRangeIndexes.Contains(_playerActions.HoveredTile) ? _moveRangeColor : _sprintRangeColor;
            Color endColor = _pathLine.startColor;
            endColor.a = 0.5f;
            _pathLine.endColor = endColor;
            _pathLine.startWidth = _pathLineSize;
            _pathLine.endWidth = _pathLineSize;
        }

        private void OnDisable()
        {
            LineRendererPool.Instance.ReturnInstances(_borrowedMoveRenders);
            LineRendererPool.Instance.ReturnInstances(_borrowedSprintRenders);

            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInMoveRange);
        }
    }
}