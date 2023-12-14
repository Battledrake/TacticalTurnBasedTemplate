using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderWidget : MonoBehaviour
{
    public event Action<float> OnSliderValueChanged;

    [SerializeField] private Slider _slider;
    [SerializeField] private string _name;
    [SerializeField] private TextMeshProUGUI _sliderNameText;
    [SerializeField] private TextMeshProUGUI _sliderValueText;

    public void SetSliderValue(float value) { _slider.value = value; }

    private void OnValidate()
    {
        _sliderNameText.text = _name;
        _sliderValueText.text = _slider.value.ToString("F1");
    }

    private void Awake()
    {
        _slider.onValueChanged.AddListener(Slider_OnValueChanged);
    }

    public void Slider_OnValueChanged(float value)
    {
        _sliderValueText.text = _slider.value.ToString("F1");
        OnSliderValueChanged?.Invoke(_slider.value);
    }
}
