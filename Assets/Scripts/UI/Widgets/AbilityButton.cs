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
        public event Action<AbilityId> OnAbilityButtonSelected;
        public event Action<AbilityId> OnAbilityButtonDeselected;

        [SerializeField] private Color _defaultBorderColor;
        [SerializeField] private Color _selectedBorderColor;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _greyCover;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _cooldownText;

        private Toggle _buttonToggle;
        private Animator _animator;

        private AbilityId _abilityId;

        public AbilityId GetAbilityId() => _abilityId;

        private void Awake()
        {
            _buttonToggle = GetComponent<Toggle>();
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _buttonToggle.onValueChanged.AddListener(OnButtonToggleChanged);
        }

        private void OnDisable()
        {
            _buttonToggle.onValueChanged.RemoveListener(OnButtonToggleChanged);
        }

        public void SetCooldownValue(int value)
        {
            if (value > 0)
            {
                _greyCover.enabled = true;
                _cooldownText.enabled = true;
                _cooldownText.text = value.ToString();
            }
            else
            {
                _greyCover.enabled = false;
                _cooldownText.enabled = false;
            }
        }

        private void OnButtonToggleChanged(bool isOn)
        {
            if (isOn)
            {
                _borderImage.color = _selectedBorderColor;
                OnAbilityButtonSelected?.Invoke(_abilityId);
            }
            else
            {
                _borderImage.color = _defaultBorderColor;
                OnAbilityButtonDeselected?.Invoke(_abilityId);
            }
            if (_animator)
                _animator.SetBool("Selected", isOn);
        }

        public void InitializeButton(AbilityId abilityId, Sprite icon)
        {
            _abilityId = abilityId;
            if (icon != null)
                _icon.sprite = icon;
        }

        public void DisableButton()
        {
            _buttonToggle.SetIsOnWithoutNotify(false);
            _borderImage.color = _defaultBorderColor;

            if (_animator)
                _animator.SetBool("Selected", false);
        }
    }
}