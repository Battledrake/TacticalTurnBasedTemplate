using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTabController : MonoBehaviour
{
    [SerializeField] private List<Ability> _debugAbilities;
    [SerializeField] private AbilityButton _abilityButtonPrefab;
    [SerializeField] private Transform _abilityButtonContainer;
    public Ability ActiveAbility { get => _activeAbility; }

    private Dictionary<int, AbilityButton> _abilityButtons = new Dictionary<int, AbilityButton>();
    private Ability _activeAbility;
    private int _activeButton = -1;

    private void Awake()
    {
        if (_debugAbilities != null && _debugAbilities.Count > 0)
            _activeAbility = _debugAbilities[0];

        for (int i = 0; i < _debugAbilities.Count; i++)
        {
            AbilityButton abilityButton = Instantiate(_abilityButtonPrefab, _abilityButtonContainer);
            abilityButton.InitializeButton(this, _debugAbilities[i].name, i);
            abilityButton.OnAbilityButtonSelected += AbilityButton_OnAbilityButtonSelected;
            abilityButton.OnAbilityButtonDeselected += AbilityButton_OnAbilityButtonDeselected;

            _abilityButtons.TryAdd(i, abilityButton);
        }
    }

    private void AbilityButton_OnAbilityButtonDeselected(int buttonIndex)
    {
        if (_activeButton == buttonIndex)
        {
            _activeAbility = null;
            _activeButton = -1;
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
    }
}
