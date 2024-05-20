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

        [Header("Ability Range Settings")]
        [SerializeField] private ActionButton _showAbilityPatternsButton;
        [SerializeField] private TMP_Dropdown _abilityRangeCombo;
        [SerializeField] private SliderWidget _abilityRangeSlider;
        [SerializeField] private Toggle _abilityRangeLineOfSightToggle;
        [SerializeField] private SliderWidget _abilityRangeLineOfSightHeightSlider;

        [Header("Area of Effect Settings")]
        [SerializeField] private Toggle _areaOfEffectButton;
        [SerializeField] private TMP_Dropdown _areaOfEffectCombo;
        [SerializeField] private SliderWidget _areaOfEffectRangeSlider;
        [SerializeField] private Toggle _areaOfEffectLineOfSightToggle;
        [SerializeField] private SliderWidget _areaOfEffectLineOfSightHeightSlider;

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
                abilityButton.InitializeButton(this, _debugAbilities[i].Name, i, _debugAbilities[i].Icon);
                abilityButton.OnAbilityButtonSelected += AbilityButton_OnAbilityButtonSelected;
                abilityButton.OnAbilityButtonDeselected += AbilityButton_OnAbilityButtonDeselected;

                _abilityButtons.TryAdd(i, abilityButton);
            }
            _showAbilityPatternsButton.OnButtonToggled += OnShowAbilityRangePatternsButtonToggled;

            _abilityRangeCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _abilityRangeCombo.onValueChanged.AddListener(OnAbilityRangePatternChanged);

            _abilityRangeSlider.OnSliderValueChanged += OnAbilityRangeChanged;
            _abilityRangeLineOfSightToggle.onValueChanged.AddListener(OnAbilityRangeLineOfSightToggled);
            _abilityRangeLineOfSightHeightSlider.OnSliderValueChanged += OnAbilityRangeLineOfSightHeightChanged;

            _areaOfEffectButton.onValueChanged.AddListener(OnAreaOfEffectButtonToggled);

            _areaOfEffectCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _areaOfEffectCombo.onValueChanged.AddListener(OnAreaOfEffectPatternChanged);

            _areaOfEffectRangeSlider.OnSliderValueChanged += OnAreaOfEffectRangeChanged;
            _areaOfEffectLineOfSightToggle.onValueChanged.AddListener(OnAreaOfEffectLineOfSightToggled);
            _areaOfEffectLineOfSightHeightSlider.OnSliderValueChanged += OnAreaOfEffectLineOfSightHeightChanged;
        }

        private void OnAreaOfEffectButtonToggled(bool isOn)
        {
            if (!isOn)
            {
                _areaOfEffectCombo.SetValueWithoutNotify(0);
                _areaOfEffectLineOfSightHeightSlider.SetSliderValueWithoutNotify(0.5f);
                ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
                if (showPatternAction)
                {
                    showPatternAction.AreaOfEffectPattern = AbilityRangePattern.None;

                    showPatternAction.ShowAbilityAreaOfEffectPattern();
                }
            }
        }

        private void OnAreaOfEffectLineOfSightHeightChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.AreaOfEffectLoSHeight = value;

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnAreaOfEffectLineOfSightToggled(bool isOn)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.AreaOfEffectRequireLoS = isOn;

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnAreaOfEffectRangeChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                if (sliderIndex == 0)
                    showPatternAction.AreaOfEffectRangeMinMax = new Vector2Int((int)value, showPatternAction.AreaOfEffectRangeMinMax.y);
                else if (sliderIndex == 1)
                    showPatternAction.AreaOfEffectRangeMinMax = new Vector2Int(showPatternAction.AreaOfEffectRangeMinMax.x, (int)value);

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnAreaOfEffectPatternChanged(int onTargetPattern)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction && onTargetPattern > 0)
            {
                _areaOfEffectRangeSlider.SetSliderValueWithoutNotify(showPatternAction.AreaOfEffectRangeMinMax);
                showPatternAction.AreaOfEffectPattern = (AbilityRangePattern)onTargetPattern;
                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnAbilityRangeLineOfSightHeightChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.RangeLineOfSightHeight = value;

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnAbilityRangeLineOfSightToggled(bool isOn)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.RangeLineOfSight = isOn;

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnShowAbilityRangePatternsButtonToggled(bool isOn)
        {
            if (!isOn)
            {
                _abilityRangeCombo.SetValueWithoutNotify(0);
                _abilityRangeLineOfSightHeightSlider.SetSliderValueWithoutNotify(0.5f);

                _areaOfEffectButton.isOn = isOn;
            }
        }

        private void OnAbilityRangeChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                if (sliderIndex == 0)
                    showPatternAction.RangeMinMax = new Vector2Int((int)value, showPatternAction.RangeMinMax.y);
                else if (sliderIndex == 1)
                    showPatternAction.RangeMinMax = new Vector2Int(showPatternAction.RangeMinMax.x, (int)value);

                showPatternAction.ShowAbilityRangePattern();
            }
        }

        private void OnAbilityRangePatternChanged(int toTargetPattern)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction && toTargetPattern > 0)
            {
                _abilityRangeSlider.SetSliderValueWithoutNotify(showPatternAction.RangeMinMax);
                showPatternAction.RangePattern = (AbilityRangePattern)toTargetPattern;

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