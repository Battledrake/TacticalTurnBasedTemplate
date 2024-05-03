using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TTBTk
{
    /// <summary>
    /// Combines a slider with a value text that displays inside the slider
    /// </summary>
    [System.Serializable]
    public struct SliderLinkers
    {
        public Slider slider;
        public TextMeshProUGUI sliderValueText;
    }

    public class SliderWidget : MonoBehaviour
    {
        /// <summary>
        /// Int represents which slider is sending the event. 0 = first or x, 1 = second or y, 2 = third or z
        /// </summary>
        public event Action<int, float> OnSliderValueChanged;

        [SerializeField] private SliderLinkers[] _sliderLinkers;
        [SerializeField] private string _name;
        [SerializeField] private TextMeshProUGUI _sliderNameText;

        public void SetSliderValue(float value, int sliderIndex = 0) { _sliderLinkers[sliderIndex].slider.SetValueWithoutNotify(value); }

        public void SetSliderValue(Vector2 value)
        {
            _sliderLinkers[0].slider.SetValueWithoutNotify(value.x);
            _sliderLinkers[1].slider.SetValueWithoutNotify(value.y);
        }

        public void SetSliderValue(Vector3 value)
        {
            _sliderLinkers[0].slider.SetValueWithoutNotify(value.x);
            _sliderLinkers[1].slider.SetValueWithoutNotify(value.y);
            _sliderLinkers[2].slider.SetValueWithoutNotify(value.z);
        }

        private void OnValidate()
        {
            _sliderNameText.text = _name;
            for (int i = 0; i < _sliderLinkers.Length; i++)
            {
                _sliderLinkers[i].sliderValueText.text = _sliderLinkers[i].slider.value.ToString("F1");
            }
        }

        private void Awake()
        {
            for (int i = 0; i < _sliderLinkers.Length; ++i)
            {
                int localCopy = i; //Fixes a unique issue where the index increments by the time the delegate gets called due to reference capture?
                _sliderLinkers[localCopy].slider.onValueChanged.AddListener(delegate { Slider_OnValueChanged(localCopy); });
            }
        }

        public void Slider_OnValueChanged(int index)
        {
            _sliderLinkers[index].sliderValueText.text = _sliderLinkers[index].slider.value.ToString("F1");
            OnSliderValueChanged?.Invoke(index, _sliderLinkers[index].slider.value);
        }
    }
}
