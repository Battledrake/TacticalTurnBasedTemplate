using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilityBarController : MonoBehaviour
    {
        public event Action<Ability> OnSelectedAbilityChanged;

        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;
        //Use ToggleGroup on AbilityButtonContainer if not using DebugMenu
        [SerializeField] private ToggleGroup _abilityBarToggleGroup;
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
        }

        public void HideBar()
        {
            _abilityBarToggleGroup.SetAllTogglesOff();
            _abilityButtonContainer.gameObject.SetActive(false);
        }
        public void ShowBar()
        {
            _abilityButtonContainer.gameObject.SetActive(true);
        }

        private void PlayerActions_OnSelectedUnitChanged(Unit unit)
        {
            ClearBar();

            if (unit == null)
                return;

            _abilitySystem = unit.GetAbilitySystem();

            if (_abilitySystem != null)
            {
                PopulateBar(_abilitySystem.GetAllAbilities());
            }
            ShowBar();
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
            if(_abilityButtons.TryGetValue(index, out AbilityButton abilityButton))
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