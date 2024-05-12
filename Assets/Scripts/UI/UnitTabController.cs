using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class UnitTabController : MonoBehaviour
    {
        [SerializeField] private UnitButton _buttonPrefab;
        [SerializeField] private Transform _buttonParent;

        [SerializeField] private PlayerActions _playerActions;

        private Dictionary<UnitType, UnitButton> _iconButtons = new Dictionary<UnitType, UnitButton>();

        private int _activeButton = -1;

        private void Awake()
        {
            UnitData[] unitData = DataManager.GetAllUnitData();
            for (int i = 0; i < unitData.Length; i++)
            {
                //TODO: We should have an option to get all data not just one at a time, so we're not calling it a bunch of times.
                UnitButton unitButton = Instantiate(_buttonPrefab, _buttonParent);
                unitButton.InitializeButton(unitData[i].unitType, unitData[i].unitIcon, _playerActions);
                unitButton.OnUnitButtonToggled += OnUnitButtonToggled;

                _iconButtons.Add(unitData[i].unitType, unitButton);
            }
        }

        private void OnUnitButtonToggled(UnitType unitButton)
        {
            //This mean we're passing in the same button.
            if (_activeButton >= 0 && (UnitType)_activeButton == unitButton)
            {
                _activeButton = -1;
            }
            else
            {
                //This means we had a valid button but it's not the one that called this. We need to disable it.
                if (_activeButton >= 0)
                {
                    _iconButtons[(UnitType)_activeButton].DisableButton();
                }
                //Now we set the new active button to the one that sent the message.
                _activeButton = (int)unitButton;
            }
        }

        public void DeselectAllUnitButtons(bool isActionActive)
        {
            if (isActionActive)
                return;

            for(int i = 0; i < _iconButtons.Count; i++)
            {
                _iconButtons[(UnitType)i].DisableButton();
            }
        }
    }
}