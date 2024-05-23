using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilityBarController : MonoBehaviour
    {
        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;
        [SerializeField] private ToggleGroup _abilityBarToggleGroup;

        [SerializeField] private PlayerActions _playerActions;

        private Dictionary<int, AbilityButton> _abilityButtons = new Dictionary<int, AbilityButton>();
        private List<Ability> _abilities = new List<Ability>();

        private void Start()
        {
            _playerActions.OnSelectedUnitChanged += PlayerActions_OnSelectedUnitChanged;
        }

        private void PlayerActions_OnSelectedUnitChanged(Unit unit)
        {
            ClearBar();

            if(unit != null)
            {
                PopulateBar(unit.UnitData.unitStats.abilities);
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
            for(int i = 0; i < abilities.Count; i++)
            {
                AbilityButton newButton = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
                newButton.InitializeButton(i, abilities[i].Icon);
                _abilityButtons.TryAdd(i, newButton);

                Toggle newButtonToggle = newButton.GetComponent<Toggle>();
                newButtonToggle.group = _abilityBarToggleGroup;
                _abilityBarToggleGroup.RegisterToggle(newButtonToggle);

                newButton.OnAbilityButtonSelected += AbilityButton_OnAbilityButtonSelected;
                newButton.OnAbilityButtonDeselected += AbilityButton_OnAbilityButtonDeselected;
            }
            _abilities = abilities;
        }

        private void AbilityButton_OnAbilityButtonSelected(int index)
        {
            _playerActions.CurrentAbility = _abilities[index];
        }

        private void AbilityButton_OnAbilityButtonDeselected(int index)
        {
            _playerActions.CurrentAbility = null;
        }
    }
}