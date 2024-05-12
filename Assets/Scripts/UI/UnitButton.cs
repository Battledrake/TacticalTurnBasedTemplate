using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class UnitButton : MonoBehaviour
    {
        public event Action<UnitData> OnUnitButtonToggled;

        [SerializeField] private Color _defaultBorderColor;
        [SerializeField] private Color _selectedBorderColor;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Toggle _buttonToggle;
        [SerializeField] private Image _icon;
        private UnitData _unitData;

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
            Debug.Log("Test");
            if (isOn)
            {
                _borderImage.color = _selectedBorderColor;
            }
            else
            {
                _borderImage.color = _defaultBorderColor;
            }
            OnUnitButtonToggled?.Invoke(_unitData);
        }

        public void InitializeButton(UnitData unitData)
        {
            _unitData = unitData;
            _icon.sprite = unitData.unitIcon;
        }

        public void DisableButton()
        {
            _buttonToggle.SetIsOnWithoutNotify(false);
            _borderImage.color = _defaultBorderColor;
        }
    }
}