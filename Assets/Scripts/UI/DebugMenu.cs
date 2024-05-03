using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleDrakeCreations.TTBTk
{
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
            _moveSpeedSlider.SetSliderValue(_cameraController.MoveSpeed);
            _rotationSpeedSlider.SetSliderValue(_cameraController.RotationSpeed);
            _zoomSpeedSlider.SetSliderValue(_cameraController.ZoomSpeed);
            _zoomMinSlider.SetSliderValue(_cameraController.ZoomMinimum);
            _zoomMaxSlider.SetSliderValue(_cameraController.ZoomMaximum);
            _moveSpeedSlider.OnSliderValueChanged += MoveSpeedSlider_OnSliderValueChanged;
            _rotationSpeedSlider.OnSliderValueChanged += RotationSpeedSlider_OnSliderValueChanged;
            _zoomSpeedSlider.OnSliderValueChanged += ZoomSpeedSlider_OnSliderValueChanged;
            _zoomMinSlider.OnSliderValueChanged += ZoomMinSlider_OnSliderValueChanged;
            _zoomMaxSlider.OnSliderValueChanged += ZoomMaxSlider_OnSliderValueChanged;
        }

        private void ZoomMaxSlider_OnSliderValueChanged(int sliderIndex, float value)
        {
            _cameraController.ZoomMaximum = value;
        }

        private void ZoomMinSlider_OnSliderValueChanged(int sliderIndex, float value)
        {
            _cameraController.ZoomMinimum = value;
        }

        private void ZoomSpeedSlider_OnSliderValueChanged(int sliderIndex, float value)
        {
            _cameraController.ZoomSpeed = value;
        }

        private void RotationSpeedSlider_OnSliderValueChanged(int sliderIndex, float value)
        {
            _cameraController.RotationSpeed = value;
        }

        private void MoveSpeedSlider_OnSliderValueChanged(int sliderIndex, float value)
        {
            _cameraController.MoveSpeed = value;
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
}
