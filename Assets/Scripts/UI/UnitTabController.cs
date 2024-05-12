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
        private Dictionary<UnitType, UnitButton> _iconButtons = new Dictionary<UnitType, UnitButton>();

        private UnitData _selectedData;

        private void Awake()
        {
            UnitData[] unitData = DataManager.GetAllUnitData();
            for(int i = 0; i < unitData.Length; i++)
            {
                //TODO: We should have an option to get all data not just one at a time, so we're not calling it a bunch of times.
                UnitButton unitButton = Instantiate(_buttonPrefab, _buttonParent);
                unitButton.InitializeButton(unitData[i]);
                unitButton.OnUnitButtonToggled += OnUnitButtonToggled;

                _iconButtons.Add(unitData[i].unitType, unitButton);
            }
        }

        private void OnUnitButtonToggled(UnitData unitData)
        {
            if(unitData == _selectedData)
            {
                _selectedData = null;
            }
            else
            {
                if (_selectedData != null)
                    _iconButtons[_selectedData.unitType].DisableButton();

                _selectedData = unitData;
            }
        }
    }
}