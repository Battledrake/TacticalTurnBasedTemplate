using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class PlayerAbilityUIController : MonoBehaviour
    {
        public event Action<Ability> OnSelectedAbilityChanged;

        [Header("Ability Buttons")]
        [SerializeField] private AbilityButton _abilityButtonPrefab;
        [SerializeField] private Transform _abilityButtonContainer;
        //Use ToggleGroup on AbilityButtonContainer if not using DebugMenu
        [SerializeField] private ToggleGroup _abilityBarToggleGroup;

        [Header("Action Points")]
        [SerializeField] private int _defaultActionPoints = 2;
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
        }

        public void HideVisuals()
        {
            _abilityBarToggleGroup.SetAllTogglesOff();
            _abilityButtonContainer.gameObject.SetActive(false);
            _actionPointContainer.gameObject.SetActive(false);
        }
        public void DisplayVisuals()
        {
            _abilityButtonContainer.gameObject.SetActive(true);
            _actionPointContainer.gameObject.SetActive(true);
            UpdateActionPointDisplay(_abilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints));
            UpdateCooldownDisplays();
        }

        private void UpdateCooldownDisplays()
        {
            foreach (KeyValuePair<int, AbilityButton> abilityButtonPair in _abilityButtons)
            {
                int abilityCooldown = _abilitySystem.GetAbility(abilityButtonPair.Value.GetAbilityId()).GetActiveCooldown();
                Debug.Log($"Ability: {abilityButtonPair.Value.GetAbilityId()}, Cooldown: {abilityCooldown}");
                if (abilityCooldown > 0)
                {
                    abilityButtonPair.Value.GetComponent<Toggle>().interactable = false;
                }
                else
                {
                    abilityButtonPair.Value.GetComponent<Toggle>().interactable = true;
                }
                abilityButtonPair.Value.SetCooldownValue(abilityCooldown);
            }
        }

        private void UpdateActionPointDisplay(int currentValue)
        {
            if (currentValue > _defaultActionPoints)
            {
                for (int i = _defaultActionPoints; i < currentValue; i++)
                {
                    _actionPointDisplays[i].transform.parent.gameObject.SetActive(true);
                }
            }
            else
            {

                for (int i = _defaultActionPoints; i < _actionPointDisplays.Count; i++)
                {
                    if (_actionPointDisplays[i].transform.parent.gameObject.activeInHierarchy)
                        _actionPointDisplays[i].transform.parent.gameObject.SetActive(false);
                }

                for (int i = 0; i < _defaultActionPoints; i++)
                {
                    _actionPointDisplays[i].enabled = true;
                }

                for (int i = _defaultActionPoints; i > currentValue; i--)
                {
                    _actionPointDisplays[i - 1].enabled = false;
                }
            }
        }

        private void PlayerActions_OnSelectedUnitChanged(Unit unit)
        {
            ClearBar();

            if (unit == null)
                return;

            _abilitySystem = unit.GetAbilitySystem();

            if (_abilitySystem != null)
            {
                PopulateAbilityBar(_abilitySystem.GetAllAbilities());

                UpdateCooldownDisplays();

                DisplayVisuals();
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

        public void PopulateAbilityBar(List<Ability> abilities)
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
                if (_abilitySystem.GetAbility(_abilityButtons[index].GetAbilityId()).GetActiveCooldown() > 0) return;

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