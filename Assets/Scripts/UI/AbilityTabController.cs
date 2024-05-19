using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilityTabController : MonoBehaviour
    {
        [SerializeField] private List<Ability> _debugAbilities;

        [Header("AbilityButtons")]
        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;

        [Header("To Target Ability Settings")]
        [SerializeField] private ActionButton _abilityToTargetButton;
        [SerializeField] private TMP_Dropdown _abilityToTargetCombo;
        [SerializeField] private SliderWidget _abilityToTargetRangeSlider;
        [SerializeField] private Toggle _abilityToTargetLoSToggle;
        [SerializeField] private SliderWidget _abilityToTargetLoSHeightSlider;

        [Header("On Target Ability Settings")]
        [SerializeField] private TMP_Dropdown _abilityOnTargetCombo;
        [SerializeField] private SliderWidget _abilityOnTargetRangeSlider;
        [SerializeField] private Toggle _abilityOnTargetLoSToggle;
        [SerializeField] private SliderWidget _abilityOnTargetLoSHeightSlider;

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
            _abilityToTargetButton.OnButtonToggled += OnShowAbilityPatternToggled;

            _abilityToTargetCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _abilityToTargetCombo.onValueChanged.AddListener(OnAbilityToTargetPatternChanged);

            _abilityToTargetRangeSlider.OnSliderValueChanged += OnAbilityToTargetRangeSliderChanged;
            _abilityToTargetLoSToggle.onValueChanged.AddListener(OnAbilityToTargetLoSToggled);
            _abilityToTargetLoSHeightSlider.OnSliderValueChanged += OnAbilityToTargetLoSSliderChanged;

            _abilityOnTargetCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _abilityOnTargetCombo.onValueChanged.AddListener(OnAbilityOnTargetPatternChanged);

            _abilityOnTargetRangeSlider.OnSliderValueChanged += OnAbilityOnTargetRangeSliderChanged;
            _abilityOnTargetLoSToggle.onValueChanged.AddListener(OnAbilityOnTargetLoSToggled);
            _abilityOnTargetLoSHeightSlider.OnSliderValueChanged += OnAbilityOnTargetLoSSliderChanged;
        }

        private void OnAbilityOnTargetLoSSliderChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.OnTargetLineOfSightHeight = value;

                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnAbilityOnTargetLoSToggled(bool isOn)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.OnTargetRequireLineOfSight = isOn;

                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnAbilityOnTargetRangeSliderChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                if (sliderIndex == 0)
                    showPatternAction.OnTargetRangeMinMax = new Vector2Int((int)value, showPatternAction.OnTargetRangeMinMax.y);
                else if (sliderIndex == 1)
                    showPatternAction.OnTargetRangeMinMax = new Vector2Int(showPatternAction.OnTargetRangeMinMax.x, (int)value);

                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnAbilityOnTargetPatternChanged(int onTargetPattern)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction && onTargetPattern > 0)
            {
                _abilityOnTargetRangeSlider.SetSliderValueWithoutNotify(showPatternAction.OnTargetRangeMinMax);
                showPatternAction.AbilityOnTargetPattern = (AbilityRangePattern)onTargetPattern;
                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnAbilityToTargetLoSSliderChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.ToTargetLineOfSightHeight = value;

                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnAbilityToTargetLoSToggled(bool isOn)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.ToTargetRequireLineOfSight = isOn;

                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnShowAbilityPatternToggled(bool isDown)
        {
            if (!isDown)
            {
                _abilityToTargetCombo.SetValueWithoutNotify(0);
                _abilityToTargetLoSHeightSlider.SetSliderValueWithoutNotify(0.5f);
            }
        }

        private void OnAbilityToTargetRangeSliderChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if(showPatternAction)
            {
                if (sliderIndex == 0)
                    showPatternAction.ToTargetRangeMinMax = new Vector2Int((int)value, showPatternAction.ToTargetRangeMinMax.y);
                else if (sliderIndex == 1)
                    showPatternAction.ToTargetRangeMinMax = new Vector2Int(showPatternAction.ToTargetRangeMinMax.x, (int)value);

                showPatternAction.ShowAbilityToTargetRangePattern();
            }
        }

        private void OnAbilityToTargetPatternChanged(int toTargetPattern)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction && toTargetPattern > 0)
            {
                _abilityToTargetRangeSlider.SetSliderValueWithoutNotify(showPatternAction.ToTargetRangeMinMax);
                showPatternAction.AbilityToTargetPattern = (AbilityRangePattern)toTargetPattern;

                showPatternAction.ShowAbilityToTargetRangePattern();
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