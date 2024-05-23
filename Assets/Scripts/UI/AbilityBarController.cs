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

        private Dictionary<int, AbilityButton> _abilities = new Dictionary<int, AbilityButton>();

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
            for (int i = 0; i < _abilities.Count; i++)
            {
                _abilityBarToggleGroup.UnregisterToggle(_abilities[i].GetComponent<Toggle>());
                Destroy(_abilities[i].gameObject);
            }
            _abilities.Clear();
        }

        public void PopulateBar(List<Ability> abilities)
        {
            for(int i = 0; i < abilities.Count; i++)
            {
                AbilityButton newButton = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
                newButton.InitializeButton(i, abilities[i].Icon);
                _abilities.TryAdd(i, newButton);

                Toggle newButtonToggle = newButton.GetComponent<Toggle>();
                newButtonToggle.group = _abilityBarToggleGroup;
                _abilityBarToggleGroup.RegisterToggle(newButtonToggle);
            }
        }
    }
}