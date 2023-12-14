using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private Button[] _tabButtons;
    [SerializeField] private Color _tabBaseColor;
    [SerializeField] private Color _tabSelectedColor;

    [SerializeField] private CameraController _cameraController;

    [SerializeField] private SliderWidget _moveSpeedSlider;
    [SerializeField] private SliderWidget _rotationSpeedSlider;
    [SerializeField] private SliderWidget _zoomSpeedSlider;
    [SerializeField] private SliderWidget _zoomMinSlider;
    [SerializeField] private SliderWidget _zoomMaxSlider;

    private void Awake()
    {
        _moveSpeedSlider.SetSliderValue(_cameraController.GetMoveSpeed());
        _rotationSpeedSlider.SetSliderValue(_cameraController.GetRotationSpeed());
        _zoomSpeedSlider.SetSliderValue(_cameraController.GetZoomSpeed());
        _zoomMinSlider.SetSliderValue(_cameraController.GetZoomMin());
        _zoomMaxSlider.SetSliderValue(_cameraController.GetZoomMax());
        _moveSpeedSlider.OnSliderValueChanged += MoveSpeedSlider_OnSliderValueChanged;
        _rotationSpeedSlider.OnSliderValueChanged += RotationSpeedSlider_OnSliderValueChanged;
        _zoomSpeedSlider.OnSliderValueChanged += ZoomSpeedSlider_OnSliderValueChanged;
        _zoomMinSlider.OnSliderValueChanged += ZoomMinSlider_OnSliderValueChanged;
        _zoomMaxSlider.OnSliderValueChanged += ZoomMaxSlider_OnSliderValueChanged;
    }

    private void ZoomMaxSlider_OnSliderValueChanged(float value)
    {
        _cameraController.SetZoomMax(value);
    }

    private void ZoomMinSlider_OnSliderValueChanged(float value)
    {
        _cameraController.SetZoomMin(value);
    }

    private void ZoomSpeedSlider_OnSliderValueChanged(float value)
    {
        _cameraController.SetZoomSpeed(value);
    }

    private void RotationSpeedSlider_OnSliderValueChanged(float value)
    {
        _cameraController.SetRotationSpeed(value);
    }

    private void MoveSpeedSlider_OnSliderValueChanged(float value)
    {
        _cameraController.SetMoveSpeed(value);
    }

    private int _activeTabIndex = -1;

    public void OnTabClicked(int tabIndex)
    {
        if (tabIndex == _activeTabIndex)
        {
            _tabButtons[_activeTabIndex].GetComponent<Image>().color = _tabBaseColor;
            _activeTabIndex = -1;
        }
        else
        {
            if (_activeTabIndex > -1)
                _tabButtons[_activeTabIndex].GetComponent<Image>().color = _tabBaseColor;

            _tabButtons[tabIndex].GetComponent<Image>().color = _tabSelectedColor;
            _activeTabIndex = tabIndex;
        }
    }
}
