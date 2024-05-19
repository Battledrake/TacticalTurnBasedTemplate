using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilityTabController : MonoBehaviour
    {
        [SerializeField] private List<Ability> _debugAbilities;

        [Header("AbilityButtons")]
        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;

        [Header("Ability Pattern Config")]
        [SerializeField] private ActionButton _abilityPatternButton;
        [SerializeField] private TMP_Dropdown _abilityPatternCombo;
        [SerializeField] private SliderWidget _abilityPatternSlider;
        [SerializeField] private Toggle _abilityRangeLoSToggle;

        [Header("Dependences")]
        [SerializeField] private PlayerActions _playerActions;
        public Ability ActiveAbility { get => _activeAbility; }

        private Dictionary<int, AbilityButton> _abilityButtons = new Dictionary<int, AbilityButton>();
        private Ability _activeAbility;
        private int _activeButton = -1;

        private void Awake()
        {
            //if (_debugAbilities != null && _debugAbilities.Count > 0)
            //    _activeAbility = _debugAbilities[0];

            for (int i = 0; i < _debugAbilities.Count; i++)
            {
                AbilityButton abilityButton = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
                abilityButton.InitializeButton(this, _debugAbilities[i].name, i);
                abilityButton.OnAbilityButtonSelected += AbilityButton_OnAbilityButtonSelected;
                abilityButton.OnAbilityButtonDeselected += AbilityButton_OnAbilityButtonDeselected;

                _abilityButtons.TryAdd(i, abilityButton);
            }
            _abilityPatternCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _abilityPatternCombo.onValueChanged.AddListener(AbilityPatternCombo_OnValueChanged);

            _abilityPatternSlider.OnSliderValueChanged += AbilityPatternSlider_OnSliderValueChanged;
            _abilityPatternButton.OnButtonToggled += AbilityPatternButton_OnButtonToggled;
            _abilityRangeLoSToggle.onValueChanged.AddListener(OnAbilityRangeLineOfSightToggled);
        }

        private void OnAbilityRangeLineOfSightToggled(bool isOn)
        {
            //Add Logic Shoon.
        }

        private void AbilityPatternButton_OnButtonToggled(bool isDown)
        {
            if (!isDown)
            {
                _abilityPatternCombo.SetValueWithoutNotify(0);
            }
        }

        private void AbilityPatternSlider_OnSliderValueChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if(showPatternAction)
            {
                if (sliderIndex == 0)
                    showPatternAction.RangeMinMax = new Vector2Int((int)value, showPatternAction.RangeMinMax.y);
                else if (sliderIndex == 1)
                    showPatternAction.RangeMinMax = new Vector2Int(showPatternAction.RangeMinMax.x, (int)value);

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void AbilityPatternCombo_OnValueChanged(int option)
        {
            _playerActions.SetLeftClickActionValue(option);
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction && option > 0)
            {
                _abilityPatternSlider.SetSliderValueWithoutNotify(showPatternAction.RangeMinMax);

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void AbilityButton_OnAbilityButtonDeselected(int buttonIndex)
        {
            if (_activeButton == buttonIndex)
            {
                _activeAbility = null;
                _activeButton = -1;
                _playerActions.CurrentAbility = null;
            }
            else
            {
                _abilityButtons[buttonIndex].DisableButton();
            }
        }

        private void AbilityButton_OnAbilityButtonSelected(int buttonIndex)
        {
            if (_activeButton >= 0)
                _abilityButtons[_activeButton].DisableButton();

            _activeAbility = _debugAbilities[buttonIndex];
            _activeButton = buttonIndex;

            _playerActions.CurrentAbility = _activeAbility;
        }

        public void SelectActiveAbilityToggled(bool isActionActive)
        {
            if (isActionActive)
                return;

            for (int i = 0; i < _abilityButtons.Count; i++)
            {
                _abilityButtons[i].DisableButton();
            }
            _activeAbility = null;
            _activeButton = -1;
            _playerActions.CurrentAbility = null;
        }
    }
}