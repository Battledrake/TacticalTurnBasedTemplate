using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _camera;

    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minZoom = 2f;
    [SerializeField] private float _maxZoom = 12f;

    public float GetMoveSpeed() { return _moveSpeed; }
    public float GetRotationSpeed() { return _rotationSpeed; }
    public float GetZoomSpeed() { return _zoomSpeed; }
    public float GetZoomMin() { return _minZoom; }
    public float GetZoomMax() { return _maxZoom; }
    public void SetMoveSpeed(float value) { _moveSpeed = value; }
    public void SetRotationSpeed(float value) { _rotationSpeed = value; }
    public void SetZoomSpeed(float value) { _zoomSpeed = value; }
    public void SetZoomMin(float value) { _minZoom = value; }
    public void SetZoomMax(float value) { _maxZoom = value; }  

    private Vector3 _targetFollowOffset;
    CinemachineTransposer _cameraTransposer;

    private void Start()
    {
        _cameraTransposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
        _targetFollowOffset = _cameraTransposer.m_FollowOffset;
    }

    private void SliderWidget_OnValueChanged(float value)
    {
        _moveSpeed = value;
    }

    private void Update()
    {
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
        _cameraTransposer.m_FollowOffset = Vector3.Lerp(_cameraTransposer.m_FollowOffset, _targetFollowOffset, Time.deltaTime * _zoomSpeed);
    }
}
