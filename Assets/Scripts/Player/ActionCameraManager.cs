using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraManager : MonoBehaviour
{
    public static ActionCameraManager Instance;

    public event Action OnActionCameraStopped;

    [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;
    [SerializeField] private CinemachineVirtualCamera _framingTransposerCamera;

    private CinemachineVirtualCamera _activeCamera;

    private bool _isActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (_isActive)
        {
            if (Vector3.Distance(Camera.main.transform.position, _activeCamera.transform.position) < 1f)
            {
                OnActionCameraStopped?.Invoke();
            }
        }
    }

    public void ShowFramingTransposerAction(Transform followTarget, Transform lookAtTarget)
    {
        _activeCamera = _framingTransposerCamera;
        SetFollowAndTargetAndActivate(followTarget, lookAtTarget);
    }

    public void ShowThirdPersonAction(Transform followTarget, Transform lookAtTarget)
    {
        _activeCamera = _thirdPersonCamera;
        SetFollowAndTargetAndActivate(followTarget, lookAtTarget);
    }

    private void SetFollowAndTargetAndActivate(Transform followTarget, Transform lookAtTarget)
    {
        _activeCamera.Follow = followTarget;
        _activeCamera.LookAt = lookAtTarget;

        _isActive = true;
        _activeCamera.gameObject.SetActive(true);
    }

    public void ShowRandomAction(Transform followTarget, Transform lookAtTarget)
    {
        _activeCamera = UnityEngine.Random.Range(0, 2) == 0 ? _thirdPersonCamera : _framingTransposerCamera;
        SetFollowAndTargetAndActivate(followTarget, lookAtTarget);
    }

    public void HideActionCamera()
    {
        _isActive = false;
        _activeCamera.gameObject.SetActive(false);
    }
}
