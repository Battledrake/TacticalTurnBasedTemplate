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
        [Tooltip("Tiles per second")]
        [SerializeField] private float _traversalSpeed = 5f;
        [Tooltip("Height difference of current and next tile before jumping is done")]
        [SerializeField] private float _heightBeforeJump = 0.2f;

        public float CurrentMovementSpeed { get => _currentMovementSpeed; }
        public bool IsMoving { get => _isMoving; }

        private TacticsGrid _tacticsGrid;

        private List<GridIndex> _currentPathToFollow = new List<GridIndex>();
        private bool _isMoving;
        private float _currentMovementSpeed;
        private float _currentAngular;

        private Matrix4x4 _previousTileTransform;
        private Matrix4x4 _nextTileTransform;

        private Vector3 _previousPosition;

        private float _traversalStep = 0f;
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
                _timeElapsed += Time.deltaTime;
                _traversalStep = _traversalSpeed * _tacticsGrid.TileSize.magnitude * Time.deltaTime;

                _currentMovementSpeed = (this.transform.position - _previousPosition).magnitude / Time.deltaTime;
                _previousPosition = this.transform.position;

                Vector3 jumpVector = Vector3.zero;
                if (ShouldJumpToNextTile())
                    jumpVector.y = _jumpCurve.Evaluate(_traversalStep);

                //transform.position = Vector3.Lerp(_previousTileTransform.GetPosition(), _nextTileTransform.GetPosition() + jumpVector, _positionAlpha.Evaluate(_timeElapsed / _traversalSpeed));
                transform.position = Vector3.MoveTowards(this.transform.position, _nextTileTransform.GetPosition() + jumpVector, _traversalStep);
                transform.rotation = Quaternion.Slerp(_previousTileTransform.rotation, _nextTileTransform.rotation, _rotationAlpha.Evaluate(_traversalSpeed * _timeElapsed));

                if (Vector3.Distance(this.transform.position, _nextTileTransform.GetPosition() + jumpVector) < 0.1f)
                {
                    _timeElapsed = 0f;
                    _traversalStep = 0f;
                    OnReachedNewTile?.Invoke(_currentPathToFollow[0]);
                    _currentPathToFollow.RemoveAt(0);
                    UpdatePath();
                }
            }
        }
    }
}