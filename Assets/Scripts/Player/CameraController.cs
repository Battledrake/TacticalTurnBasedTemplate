using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _camera;

        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _minZoom = 2f;
        [SerializeField] private float _maxZoom = 12f;
        [SerializeField] private float _minFollow = -1f;
        [SerializeField] private float _maxFollow = -20f;
        [SerializeField] private float _heightBeforeReturn = 10f;

        [SerializeField] private float _moveToTargetSpeed = 50f;

        [SerializeField] private PlayerActions _playerActions;

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }
        public float ZoomSpeed { get => _zoomSpeed; set => _zoomSpeed = value; }
        public float ZoomMinimum { get => _minZoom; set => _minZoom = value; }
        public float ZoomMaximum { get => _maxZoom; set => _maxZoom = value; }

        private Vector3 _targetFollowOffset;
        CinemachineTransposer _cameraTransposer;

        private int _currentTargetIndex = 0;
        private Vector3 _targetPosition;
        private bool _moveToTarget;


        private void Start()
        {
            _cameraTransposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
            _targetFollowOffset = _cameraTransposer.m_FollowOffset;

            _targetPosition = this.transform.position;
        }

        public void SetMoveToTarget(Vector3 moveToPosition)
        {
            _targetPosition = moveToPosition;
            _moveToTarget = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (CombatManager.Instance.UnitsInCombat.Count <= 0)
                    return;

                _currentTargetIndex = _currentTargetIndex % CombatManager.Instance.UnitsInCombat.Count;
                Unit tabSelectedUnit = CombatManager.Instance.UnitsInCombat[_currentTargetIndex];
                _targetPosition = tabSelectedUnit.transform.position;
                _targetPosition.y += 1.5f;
                _moveToTarget = true;
                _currentTargetIndex++;
                //_playerActions.SetSelectedTileAndUnit(tabSelectedUnit.UnitGridIndex);
            }

            if (_moveToTarget)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, _targetPosition, _moveToTargetSpeed * Time.deltaTime);

                if (Vector3.Distance(this.transform.position, _targetPosition) < 0.2f)
                {
                    _moveToTarget = false;
                }
                else
                {
                    return;
                }
            }

            HandleMovement();
            HandleRotation();
            HandleZoom();
        }

        private void HandleMovement()
        {
            Vector3 inputMoveDir = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.W))
            {
                inputMoveDir.z += 1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputMoveDir.x -= 1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputMoveDir.z -= 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputMoveDir.x += 1f;
            }
            Vector3 moveVector = transform.forward * inputMoveDir.z + transform.right * inputMoveDir.x;
            transform.position += moveVector * _moveSpeed * Time.deltaTime;
        }

        private void HandleRotation()
        {
            Vector3 rotationVector = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.Q))
            {
                rotationVector.y += 1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                rotationVector.y -= 1f;
            }
            transform.eulerAngles += rotationVector * _rotationSpeed * Time.deltaTime;
        }

        private void HandleZoom()
        {
            float zoomAmount = 1f;
            if (Input.mouseScrollDelta.y > 0)
            {
                _targetFollowOffset.y -= zoomAmount;
            }
            if (Input.mouseScrollDelta.y < 0)
            {
                _targetFollowOffset.y += zoomAmount;
            }
            _targetFollowOffset.y = Mathf.Clamp(_targetFollowOffset.y, _minZoom, _maxZoom);
            if (_targetFollowOffset.y > Mathf.Abs(_maxFollow))
            {
                _targetFollowOffset.z = _maxFollow - _heightBeforeReturn + (_targetFollowOffset.y + _maxFollow);
            }
            else
            {
                _targetFollowOffset.z = -_targetFollowOffset.y;
            }
            _targetFollowOffset.z = Mathf.Clamp(_targetFollowOffset.z, _maxFollow, _minFollow);
            _cameraTransposer.m_FollowOffset = Vector3.Lerp(_cameraTransposer.m_FollowOffset, _targetFollowOffset, Time.deltaTime * _zoomSpeed);
        }
    }
}
