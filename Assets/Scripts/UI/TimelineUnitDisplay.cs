using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TimelineUnitDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _selectedBackground;
        [SerializeField] private Image _defaultBackground;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Slider _healthSlider;

        [SerializeField] private int _defaultAlpha = 100;
        [SerializeField] private int _selectedAlpha = 200;

        private Unit _unit;
        private Vector2 _defaultIconSize = new Vector2(75f, 75f);
        private Vector2 _selectedIconSize = new Vector2(100f, 100f);
        private bool _isActive = false;

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

            teamColor.a = _defaultAlpha / 255f;
            _defaultBackground.color = teamColor;
            _defaultBackground.enabled = true;

            teamColor.a = _selectedAlpha / 255f;
            _selectedBackground.color = teamColor;
            _selectedBackground.enabled = false;

            _iconImage.sprite = unit.UnitData.assetData.unitIcon;
            _iconImage.GetComponent<RectTransform>().sizeDelta = _defaultIconSize;

            UpdateIconHealth(_unit.CurrentHealth, _unit.MaxHealth);

            Unit.OnAnyUnitHealthChanged += Unit_OnAnyUnitHealthChanged;
            _unit.OnUnitHoveredChanged += Unit_OnUnitHoveredChanged;
            _unit.OnUnitSelectedChanged += Unit_OnUnitSelectedChanged;
            CombatManager.Instance.OnUnitTurnEnded += CombatManager_OnUnitTurnEnded;

            _isActive = true;
        }

        private void OnDisable()
        {
            if (_unit == null)
                return;

            Unit.OnAnyUnitHealthChanged -= Unit_OnAnyUnitHealthChanged;
            _unit.OnUnitHoveredChanged -= Unit_OnUnitHoveredChanged;
            _unit.OnUnitSelectedChanged -= Unit_OnUnitSelectedChanged;
            CombatManager.Instance.OnUnitTurnEnded -= CombatManager_OnUnitTurnEnded;
        }

        private void CombatManager_OnUnitTurnEnded(Unit unit)
        {
            if (_unit == null) return;
            if (_unit != unit) return;

            if (CombatManager.Instance.TurnOrderType == TurnOrderType.Team)
            {
                Color greyed = Color.grey;
                greyed.a = _defaultAlpha / 255f;
                _defaultBackground.color = greyed;
                _isActive = false;
            }
        }

        private void Unit_OnUnitSelectedChanged(bool isSelected)
        {
            Color backgroundAlpha = _selectedBackground.color;
            if (isSelected)
            {
                _defaultBackground.enabled = false;
                _selectedBackground.enabled = true;
                _iconImage.GetComponent<RectTransform>().sizeDelta = _selectedIconSize;
                backgroundAlpha.a = _selectedAlpha / 255f;

            }
            else
            {
                _defaultBackground.enabled = true;
                _selectedBackground.enabled = false;
                _iconImage.GetComponent<RectTransform>().sizeDelta = _defaultIconSize;
                backgroundAlpha.a = _defaultAlpha / 255f;

            }
            _selectedBackground.color = backgroundAlpha;
        }

        private void Unit_OnUnitHoveredChanged(bool isHovered)
        {
            if (!_isActive)
                return;

            Color defaultAlpha = _defaultBackground.color;
            Color selectedAlpha = _selectedBackground.color;
            if (isHovered)
            {
                defaultAlpha.a = 1f;
                selectedAlpha.a = 1f;
            }
            else
            {
                defaultAlpha.a = _defaultAlpha / 255f;
                selectedAlpha.a = _selectedAlpha / 255f;
            }
            _defaultBackground.color = defaultAlpha;
            _selectedBackground.color = selectedAlpha;
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
            if (_isActive)
            {
                //TODO: Fixed one. We'll fix cameras one day. All these camera GameObject.Finds have got to go.
                GameObject.Find("[Cameras]").GetComponent<CameraController>().SetMoveToTarget(_unit.transform.position + new Vector3(0f, 1.5f, 0f));

                CombatManager.Instance.CycleUnits(_unit);
            }
        }
    }
}