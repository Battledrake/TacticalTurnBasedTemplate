using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilityButton : MonoBehaviour
    {
        public event Action<int> OnAbilityButtonSelected;
        public event Action<int> OnAbilityButtonDeselected;

        [SerializeField] private Color _defaultBorderColor;
        [SerializeField] private Color _selectedBorderColor;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _icon;

        private Toggle _buttonToggle;

        private int _abilityIndex;

        private void Awake()
        {
            _buttonToggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _buttonToggle.onValueChanged.AddListener(OnButtonToggleChanged);
        }

        private void OnDisable()
        {
            _buttonToggle.onValueChanged.RemoveListener(OnButtonToggleChanged);
        }

        private void OnButtonToggleChanged(bool isOn)
        {
            if (isOn)
            {
                _borderImage.color = _selectedBorderColor;
                OnAbilityButtonSelected?.Invoke(_abilityIndex);
            }
            else
            {
                _borderImage.color = _defaultBorderColor;
                OnAbilityButtonDeselected?.Invoke(_abilityIndex);
            }
        }

        public void InitializeButton(int abilityIndex, Sprite icon)
        {
            _abilityIndex = abilityIndex;
            if (icon != null)
                _icon.sprite = icon;
        }

        public void DisableButton()
        {
            _buttonToggle.SetIsOnWithoutNotify(false);
            _borderImage.color = _defaultBorderColor;
        }
    }
}