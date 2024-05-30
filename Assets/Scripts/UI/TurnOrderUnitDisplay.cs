using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TurnOrderUnitDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _bgImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Slider _healthSlider;

        [SerializeField] private int _teamColorAlpha = 100;

        private Unit _unit;

        public void UpdateIcon(Unit unit)
        {
            Unit.OnAnyUnitHealthChanged -= Unit_OnAnyUnitHealthChanged;
            if (_unit)
            {
                _unit.OnUnitHoveredChanged -= Unit_OnUnitHoveredChanged;
                _unit.OnUnitSelectedChanged -= Unit_OnUnitSelectedChanged;
            }


            _unit = unit;

            Color teamColor = CombatManager.Instance.GetTeamColor(unit.TeamIndex);
            teamColor.a = _teamColorAlpha / 255f;
            _bgImage.color = teamColor;

            _iconImage.sprite = unit.UnitData.assetData.unitIcon;
            UpdateIconHealth(_unit.CurrentHealth, _unit.MaxHealth);
            Unit.OnAnyUnitHealthChanged += Unit_OnAnyUnitHealthChanged;
            _unit.OnUnitHoveredChanged += Unit_OnUnitHoveredChanged;
            _unit.OnUnitSelectedChanged += Unit_OnUnitSelectedChanged;
        }

        private void Unit_OnUnitSelectedChanged(bool isSelected)
        {
            Color borderAlpha = _borderImage.color;
            if (isSelected)
            {
                this.transform.localScale = new Vector3(1.1f, 1.1f, 1);
                this.GetComponent<RectTransform>().pivot = new Vector3(2.0f, 0.5f);

                borderAlpha.a = 1f;
            }
            else
            {
                this.transform.localScale = Vector3.one;

                borderAlpha.a = _teamColorAlpha / 255f;
                this.GetComponent<RectTransform>().pivot = new Vector3(0.5f, 0.5f);
            }
            _borderImage.color = borderAlpha;
        }

        private void Unit_OnUnitHoveredChanged(bool isHovered)
        {
            Color colorAlpha = _bgImage.color;
            if (isHovered)
            {
                colorAlpha.a = 1f;
                //this.transform.localScale = new Vector3(1.1f, 1.1f, 1);
            }
            else
            {
                colorAlpha.a = _teamColorAlpha / 255f;
                //this.transform.localScale = Vector3.one;
            }
            _bgImage.color = colorAlpha;
        }

        private void Unit_OnAnyUnitHealthChanged(Unit unit)
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            eventData.Use();
            _unit.SetIsHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            eventData.Use();
            _unit.SetIsHovered(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameObject.Find("[CameraControllers]").GetComponent<CameraController>().SetMoveToTarget(_unit.transform.position + new Vector3(0f, 1.5f, 0f));
        }
    }
}