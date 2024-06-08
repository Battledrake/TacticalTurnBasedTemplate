using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilityBarController : MonoBehaviour
    {
        public event Action<Ability> OnSelectedAbilityChanged;

        [Header("Ability Buttons")]
        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;
        //Use ToggleGroup on AbilityButtonContainer if not using DebugMenu
        [SerializeField] private ToggleGroup _abilityBarToggleGroup;

        [Header("Action Point")]
        [SerializeField] private Transform _actionPointContainer;
        [SerializeField] private List<Image> _actionPointDisplays;

        [Header("Dependencies")]
        [SerializeField] private PlayerActions _playerActions;

        private Dictionary<int, AbilityButton> _abilityButtons = new Dictionary<int, AbilityButton>();

        private AbilitySystem _abilitySystem;

        private void Start()
        {
            _playerActions.OnSelectedUnitChanged += PlayerActions_OnSelectedUnitChanged;
        }

        private void OnDisable()
        {
            _playerActions.OnSelectedUnitChanged -= PlayerActions_OnSelectedUnitChanged;
            if (_abilitySystem)
            {
                _abilitySystem.OnAttributeCurrentChanged -= AbilitySystem_OnAttributeCurrentChanged;
            }
        }

        public void HideBar()
        {
            _abilityBarToggleGroup.SetAllTogglesOff();
            _abilityButtonContainer.gameObject.SetActive(false);
            _actionPointContainer.gameObject.SetActive(false);
        }
        public void ShowBar()
        {
            _abilityButtonContainer.gameObject.SetActive(true);
            _actionPointContainer.gameObject.SetActive(true);
        }

        private void PlayerActions_OnSelectedUnitChanged(Unit unit)
        {
            ClearBar();

            if (unit == null)
                return;

            if (_abilitySystem)
            {
                _abilitySystem.OnAttributeCurrentChanged -= AbilitySystem_OnAttributeCurrentChanged;
            }

            _abilitySystem = unit.GetAbilitySystem();

            if (_abilitySystem != null)
            {
                _abilitySystem.OnAttributeCurrentChanged += AbilitySystem_OnAttributeCurrentChanged;

                PopulateBar(_abilitySystem.GetAllAbilities());

                ShowBar();

                //TODO: Refactor this. Very hacky way of getting this to work. Will revisit later.
                for (int i = 0; i < 2; i++)
                {
                    _actionPointDisplays[i].enabled = true;
                }
                int currentAP = _abilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints);
                if (currentAP == 0) return;
                for(int i = 2; i > currentAP; i--)
                {
                    _actionPointDisplays[i - 1].enabled = false;
                }
            }
        }

        private void AbilitySystem_OnAttributeCurrentChanged(AttributeId attribute, int oldValue, int newValue)
        {
            if (attribute == AttributeId.ActionPoints)
            {
                for (int i = oldValue; i > newValue; i--)
                    _actionPointDisplays[i - 1].enabled = false;
            }
        }

        public void ClearBar()
        {
            for (int i = 0; i < _abilityButtons.Count; i++)
            {
                _abilityBarToggleGroup.UnregisterToggle(_abilityButtons[i].GetComponent<Toggle>());
                Destroy(_abilityButtons[i].gameObject);
            }
            _abilityButtons.Clear();
        }

        public void PopulateBar(List<Ability> abilities)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                AbilityButton newButton = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
                newButton.InitializeButton(abilities[i].GetAbilityId(), abilities[i].Icon);
                _abilityButtons.TryAdd(i, newButton);

                Toggle newButtonToggle = newButton.GetComponent<Toggle>();
                newButtonToggle.group = _abilityBarToggleGroup;
                _abilityBarToggleGroup.RegisterToggle(newButtonToggle);

                newButton.OnAbilityButtonSelected += AbilityButton_OnAbilityButtonSelected;
                newButton.OnAbilityButtonDeselected += AbilityButton_OnAbilityButtonDeselected;
            }
        }

        private void AbilityButton_OnAbilityButtonSelected(AbilityId abilityId)
        {
            OnSelectedAbilityChanged?.Invoke(_abilitySystem.GetAbility(abilityId));
        }

        private void AbilityButton_OnAbilityButtonDeselected(AbilityId abilityId)
        {
            OnSelectedAbilityChanged?.Invoke(null);
        }

        public void SetSelectedAbilityFromIndex(int index)
        {
            if (_abilityButtons.TryGetValue(index, out AbilityButton abilityButton))
            {
                Toggle abilityButtonToggle = abilityButton.GetComponent<Toggle>();
                if (abilityButtonToggle.isOn)
                    abilityButtonToggle.isOn = false;
                else
                    abilityButtonToggle.isOn = true;
            }
            else
            {
                _abilityBarToggleGroup.SetAllTogglesOff();
                OnSelectedAbilityChanged?.Invoke(null);
            }
        }
    }
}