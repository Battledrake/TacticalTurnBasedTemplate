using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class GridMovement : MonoBehaviour
    {
        public event Action OnMovementStarted;
        public event Action<GridIndex> OnReachedNewTile;
        public event Action OnReachedDestination;

        [SerializeField] private AnimationCurve _positionAlpha;
        [SerializeField] private AnimationCurve _rotationAlpha;
        [SerializeField] private AnimationCurve _jumpCurve;
        [Range(0.1f, 2f)]
        [SerializeField] private float _tileTraversalTime = 1f;
        [SerializeField] private float _heightBeforeJump = 0.2f;

        public float CurrentMovementSpeed { get => _movementSpeed; }
        public bool IsMoving { get => _isMoving; }

        private TacticsGrid _tacticsGrid;

        private List<GridIndex> _currentPathToFollow = new List<GridIndex>();
        private bool _isMoving;
        private float _movementSpeed;

        private Matrix4x4 _previousTileTransform;
        private Matrix4x4 _nextTileTransform;

        private Vector3 _previousPosition;

        private float _timeElapsed = 0f;

        public void SetPathingGrid(TacticsGrid tacticsGrid)
        {
            _tacticsGrid = tacticsGrid;
        }

        public void SetPathAndMove(List<GridIndex> path)
        {
            _currentPathToFollow = new List<GridIndex>(path);
            _isMoving = true;
            UpdatePath();

            OnMovementStarted?.Invoke();
        }

        private void UpdatePath()
        {
            if (_currentPathToFollow.Count > 0)
            {
                _previousTileTransform = this.transform.localToWorldMatrix;
                Matrix4x4 nextTransform = _tacticsGrid.GridTiles[_currentPathToFollow[0]].tileMatrix;
                Vector3 lookVector = nextTransform.GetPosition() - this.transform.position;
                lookVector.y = 0f;
                Quaternion lookRotation = Quaternion.LookRotation(lookVector, Vector3.up);

                _nextTileTransform = Matrix4x4.TRS(nextTransform.GetPosition(), lookRotation, nextTransform.lossyScale);
                _timeElapsed = 0f;
            }
            else
            {
                _isMoving = false;
                OnReachedDestination?.Invoke();
            }
        }

        private bool ShouldJumpToNextTile()
        {
            float previousY = this.transform.position.y;
            float nextY = _nextTileTransform.GetPosition().y;

            return Mathf.Abs(nextY - previousY) > _heightBeforeJump;
        }

        public void Stop()
        {
            _isMoving = false;
            _currentPathToFollow.Clear();
        }

        public void Pause(bool isPaused)
        {
            _isMoving = !isPaused;
        }

        private void Update()
        {
            if (_isMoving)
            {
                if (_timeElapsed >= _tileTraversalTime)
                {
                    OnReachedNewTile?.Invoke(_currentPathToFollow[0]);
                    _currentPathToFollow.RemoveAt(0);
                    UpdatePath();
                }
                else
                {
                    _timeElapsed += Time.deltaTime;
                }

                _movementSpeed = (this.transform.position - _previousPosition).magnitude;
                _previousPosition = this.transform.position;

                Vector3 jumpVector = Vector3.zero;
                if (ShouldJumpToNextTile())
                    jumpVector.y = _jumpCurve.Evaluate(_timeElapsed);

                transform.position = Vector3.Lerp(_previousTileTransform.GetPosition(), _nextTileTransform.GetPosition() + jumpVector, _positionAlpha.Evaluate(_timeElapsed / _tileTraversalTime));
                transform.rotation = Quaternion.Slerp(_previousTileTransform.rotation, _nextTileTransform.rotation, _rotationAlpha.Evaluate(_timeElapsed / _tileTraversalTime));
            }
        }
    }
}