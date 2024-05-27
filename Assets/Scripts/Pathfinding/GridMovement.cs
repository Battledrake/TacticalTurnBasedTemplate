using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class GridMovement : MonoBehaviour
    {
        public event Action OnMovementStarted;
        public event Action OnMovementStopped;
        public event Action<GridIndex> OnReachedNewTile;
        public event Action OnReachedDestination;

        [SerializeField] private AnimationCurve _positionAlpha;
        [SerializeField] private AnimationCurve _rotationAlpha;
        [SerializeField] private AnimationCurve _jumpCurve;
        [Tooltip("Tiles per second")]
        [SerializeField] private float _traversalSpeed = 5f;
        [Tooltip("Height difference of current and next tile before jumping is done")]
        [SerializeField] private float _heightBeforeJump = 0.2f;

        public bool IsMoving { get => _isMoving; }

        private TacticsGrid _tacticsGrid;

        private List<GridIndex> _currentPathToFollow = new List<GridIndex>();
        private bool _isMoving;

        private Matrix4x4 _previousTileTransform;
        private Matrix4x4 _nextTileTransform;

        private GridIndex _prevIndex;

        private float _traversalStep = 0f;
        private float _timeElapsed = 0f;

        private bool _isClimbingUp = false;
        private bool _isClimbingDown = false;

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
                _tacticsGrid.GetTileDataFromIndex(_currentPathToFollow[0], out TileData nextTile);
                Matrix4x4 nextTransform = nextTile.tileMatrix;
                Vector3 lookVector = nextTransform.GetPosition() - this.transform.position;
                lookVector.y = 0f;
                Quaternion lookRotation = Quaternion.LookRotation(lookVector, Vector3.up);

                _nextTileTransform = Matrix4x4.TRS(nextTransform.GetPosition(), lookRotation, nextTransform.lossyScale);

                if (nextTile.climbData.hasClimbLink)
                {
                    if (nextTile.climbData.climbLinks.Contains(_prevIndex))
                    {
                        Vector3 direction = nextTransform.GetPosition() - this.transform.position;
                        if (direction.y > 0)
                            _isClimbingUp = true;
                        else
                            _isClimbingDown = true;
                    }
                }
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
            OnMovementStopped?.Invoke();
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

                //TODO: Get this working again
                //Vector3 jumpVector = Vector3.zero;
                //if (ShouldJumpToNextTile())
                //    jumpVector.y = _jumpCurve.Evaluate(_traversalStep);

                Vector3 targetDestination = _nextTileTransform.GetPosition();
                if (_isClimbingUp)
                    targetDestination = new Vector3(this.transform.position.x, targetDestination.y, this.transform.position.z);
                if (_isClimbingDown)
                    targetDestination = new Vector3(targetDestination.x, this.transform.position.y, targetDestination.z);
                

                this.transform.position = Vector3.MoveTowards(this.transform.position, targetDestination, _traversalStep);
                this.transform.rotation = Quaternion.Slerp(_previousTileTransform.rotation, _nextTileTransform.rotation, _rotationAlpha.Evaluate(_traversalSpeed * _timeElapsed));

                if (Vector3.Distance(this.transform.position, targetDestination) < 0.1f)
                {
                    if (_isClimbingUp || _isClimbingDown)
                    {
                        _isClimbingUp = false;
                        _isClimbingDown = false;
                        return;
                    }

                    _timeElapsed = 0f;
                    _traversalStep = 0f;
                    _prevIndex = _currentPathToFollow[0];
                    OnReachedNewTile?.Invoke(_currentPathToFollow[0]);
                    _currentPathToFollow.RemoveAt(0);
                    UpdatePath();
                }
            }
        }
    }
}