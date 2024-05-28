using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TurnOrderUnitDisplay : MonoBehaviour
    {
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _bgImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Slider _healthSlider;

        private Unit _unit;

        public void UpdateIcon(Unit unit)
        {
            Unit.OnUnitHealthChanged -= Unit_OnUnitHealthChanged;
            if (_unit)
            {
                _unit.OnUnitHoveredChanged -= Unit_OnUnitHoveredChanged;
                _unit.OnUnitSelectedChanged -= Unit_OnUnitSelectedChanged;
            }


            _unit = unit;

            _bgImage.color = CombatManager.Instance.GetTeamColor(unit.TeamIndex);
            _iconImage.sprite = unit.UnitData.assetData.unitIcon;
            UpdateIconHealth(_unit.CurrentHealth, _unit.MaxHealth);
            Unit.OnUnitHealthChanged += Unit_OnUnitHealthChanged;
            _unit.OnUnitHoveredChanged += Unit_OnUnitHoveredChanged;
            _unit.OnUnitSelectedChanged += Unit_OnUnitSelectedChanged;
        }

        private void Unit_OnUnitSelectedChanged(bool isSelected)
        {
            if (isSelected)
            {
                _borderImage.color = Color.white;
            }
            else
            {
                _borderImage.color = _bgImage.color;
            }
        }

        private void Unit_OnUnitHoveredChanged(bool isHovered)
        {
            if (isHovered)
            {
                this.transform.localScale = new Vector3(1.1f, 1.1f, 1);
            }
            else
            {
                this.transform.localScale = Vector3.one;
            }
        }

        private void Unit_OnUnitHealthChanged(Unit unit)
        {
            if (_unit == unit)
                UpdateIconHealth(_unit.CurrentHealth, _unit.MaxHealth);
        }

        public void UpdateIconHealth(int currentHealth, int maxHealth)
        {
            _healthText.text = currentHealth + " / " + maxHealth;
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = currentHealth;
        }
    }
}