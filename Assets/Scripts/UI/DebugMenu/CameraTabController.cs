using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CameraTabController : MonoBehaviour
    {
        [Header("Camera Sliders")]
        [SerializeField] private SliderWidget _moveSpeedSlider;
        [SerializeField] private SliderWidget _rotationSpeedSlider;
        [SerializeField] private SliderWidget _zoomSpeedSlider;
        [SerializeField] private SliderWidget _zoomMinSlider;
        [SerializeField] private SliderWidget _zoomMaxSlider;

        [Header("Dependencies")]
        [SerializeField] private CameraController _cameraController;

        private void Awake()
        {
            _moveSpeedSlider.SetSliderValueWithoutNotify(_cameraController.MoveSpeed);
            _rotationSpeedSlider.SetSliderValueWithoutNotify(_cameraController.RotationSpeed);
            _zoomSpeedSlider.SetSliderValueWithoutNotify(_cameraController.ZoomSpeed);
            _zoomMinSlider.SetSliderValueWithoutNotify(_cameraController.ZoomMinimum);
            _zoomMaxSlider.SetSliderValueWithoutNotify(_cameraController.ZoomMaximum);
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
    }
}