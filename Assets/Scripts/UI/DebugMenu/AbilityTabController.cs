using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [RequireComponent(typeof(AbilitySystem))]
    public class AbilityTabController : MonoBehaviour, IAbilitySystem
    {
        [SerializeField] private List<AbilityId> _debugAbilities;

        [Header("AbilityButtons")]
        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;
        [SerializeField] private ToggleGroup _abilityButtonsToggleGroup;

        [Header("Ability Range Settings")]
        [SerializeField] private ActionButton _showAbilityPatternsButton;
        [SerializeField] private TMP_Dropdown _abilityRangeCombo;
        [SerializeField] private SliderWidget _abilityRangeSlider;
        [SerializeField] private Toggle _abilityRangeLineOfSightToggle;
        [SerializeField] private SliderWidget _abilityRangeLineOfSightHeightSlider;
        [SerializeField] private SliderWidget _abilityRangeLoSOffsetDistanceSlider;

        [Header("Area of Effect Settings")]
        [SerializeField] private Toggle _areaOfEffectButton;
        [SerializeField] private TMP_Dropdown _areaOfEffectCombo;
        [SerializeField] private SliderWidget _areaOfEffectRangeSlider;
        [SerializeField] private Toggle _areaOfEffectLineOfSightToggle;
        [SerializeField] private SliderWidget _areaOfEffectLineOfSightHeightSlider;
        [SerializeField] private SliderWidget _areaOfEffectLoSOffsetDistanceSlider;

        [Header("Dependences")]
        [SerializeField] private PlayerActions _playerActions;

        public ToggleGroup AbilityButtonsToggleGroup { get => _abilityButtonsToggleGroup; }
        public Ability ActiveAbility { get => _activeAbility; }

        private Dictionary<AbilityId, AbilityButton> _abilityButtons = new Dictionary<AbilityId, AbilityButton>();
        private Ability _activeAbility;
        private int _activeButton = -1;
        private AbilitySystem _abilitySystem;

        private void Awake()
        {
            _abilitySystem = this.GetComponent<AbilitySystem>();
            _abilitySystem.InitAbilitySystem(null, null, _debugAbilities);

            List<Ability> initializedAbilities = _abilitySystem.GetAbilities();
            for(int i = 0; i < initializedAbilities.Count; i++)
            {
                _abilitySystem.GetAbilities()[i].SetCheat(true); 
                
                AbilityButton abilityButton = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
                abilityButton.InitializeButton(initializedAbilities[i].GetAbilityId(), initializedAbilities[i].Icon);
                abilityButton.OnAbilityButtonSelected += AbilityButton_OnAbilityButtonSelected;
                abilityButton.OnAbilityButtonDeselected += AbilityButton_OnAbilityButtonDeselected;

                _abilityButtons.TryAdd(initializedAbilities[i].GetAbilityId(), abilityButton);

                _abilityButtonsToggleGroup.RegisterToggle(abilityButton.GetComponent<Toggle>());
                abilityButton.GetComponent<Toggle>().group = _abilityButtonsToggleGroup;
            }

            _showAbilityPatternsButton.OnButtonToggled += OnShowAbilityRangePatternsButtonToggled;

            _abilityRangeCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _abilityRangeCombo.onValueChanged.AddListener(OnAbilityRangePatternChanged);

            _abilityRangeSlider.OnSliderValueChanged += OnAbilityRangeChanged;
            _abilityRangeLineOfSightToggle.onValueChanged.AddListener(OnAbilityRangeLineOfSightToggled);
            _abilityRangeLineOfSightHeightSlider.OnSliderValueChanged += OnAbilityRangeLineOfSightHeightChanged;
            _abilityRangeLoSOffsetDistanceSlider.OnSliderValueChanged += OnAbilityRangeLoSOffsetDistanceChanged;

            _areaOfEffectButton.onValueChanged.AddListener(OnAreaOfEffectButtonToggled);

            _areaOfEffectCombo.AddOptions(Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().Select(p => p.ToString()).ToList());
            _areaOfEffectCombo.onValueChanged.AddListener(OnAreaOfEffectPatternChanged);

            _areaOfEffectRangeSlider.OnSliderValueChanged += OnAreaOfEffectRangeChanged;
            _areaOfEffectLineOfSightToggle.onValueChanged.AddListener(OnAreaOfEffectLineOfSightToggled);
            _areaOfEffectLineOfSightHeightSlider.OnSliderValueChanged += OnAreaOfEffectLineOfSightHeightChanged;
            _areaOfEffectLoSOffsetDistanceSlider.OnSliderValueChanged += OnAreaOfEffectLoSOffsetDistanceChanged;
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

        private void OnAbilityRangeLineOfSightToggled(bool isOn)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.RangeLineOfSight = isOn;

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
        private void OnAbilityRangeLoSOffsetDistanceChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.RangeLineOfSightOffsetDistance = value;

                showPatternAction.ShowAbilityRangePattern();
            }
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

        private void OnAreaOfEffectLineOfSightToggled(bool isOn)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.AreaOfEffectRequireLoS = isOn;

                showPatternAction.ShowAbilityRangePattern();
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

        private void OnAreaOfEffectLoSOffsetDistanceChanged(int sliderIndex, float value)
        {
            ShowAbilityPatternAction showPatternAction = _playerActions.LeftClickAction?.GetComponent<ShowAbilityPatternAction>();
            if (showPatternAction)
            {
                showPatternAction.AreaOfEffectLoSOffsetDistance = value;

                showPatternAction.ShowAbilityAreaOfEffectPattern();
            }
        }

        private void AbilityButton_OnAbilityButtonDeselected(AbilityId abilityId)
        {
            if (_activeButton == (int)abilityId)
            {
                _activeAbility = null;
                _activeButton = -1;
                _playerActions.CurrentAbility = null;
            }
            else
            {
                _abilityButtons[abilityId].DisableButton();
            }
        }

        private void AbilityButton_OnAbilityButtonSelected(AbilityId abilityId)
        {
            if (_activeButton >= 0)
                _abilityButtons[(AbilityId)_activeButton].DisableButton();

            _activeAbility = _abilitySystem.GetAbility(abilityId);
            _activeButton = (int)abilityId;

            _playerActions.CurrentAbility = _activeAbility;
        }

        public void SelectActiveAbilityToggled(bool isActionActive)
        {
            if (isActionActive)
                return;

            foreach (KeyValuePair<AbilityId, AbilityButton> buttonPair in _abilityButtons)
            {
                buttonPair.Value.DisableButton();
            }

            _activeAbility = null;
            _activeButton = -1;
            _playerActions.CurrentAbility = null;
        }

        public AbilitySystem AbilitySystem => _abilitySystem;
    }
}