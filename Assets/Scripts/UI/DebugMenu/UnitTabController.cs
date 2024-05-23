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
        [SerializeField] private UnitButton _unitButtonPrefab;
        [SerializeField] private Transform _unitButtonContainer;

        [SerializeField] private PlayerActions _playerActions;

        private Dictionary<UnitId, UnitButton> _iconButtons = new Dictionary<UnitId, UnitButton>();

        private int _activeButton = -1;

        private void Awake()
        {
            UnitData[] unitData = DataManager.GetAllUnitData();
            for (int i = 0; i < unitData.Length; i++)
            {
                UnitButton unitButton = Instantiate(_unitButtonPrefab, _unitButtonContainer);
                unitButton.InitializeButton(unitData[i].unitId, unitData[i].assetData.unitIcon, _playerActions);
                unitButton.OnUnitButtonToggled += OnUnitButtonToggled;

                _iconButtons.Add(unitData[i].unitId, unitButton);
            }
        }

        private void OnUnitButtonToggled(UnitId unitButton)
        {
            //This mean we're passing in the same button.
            if (_activeButton >= 0 && (UnitId)_activeButton == unitButton)
            {
                _activeButton = -1;
            }
            else
            {
                //This means we had a valid button but it's not the one that called this. We need to disable it.
                if (_activeButton >= 0)
                {
                    _iconButtons[(UnitId)_activeButton].DisableButton();
                }
                //Now we set the new active button to the one that sent the message.
                _activeButton = (int)unitButton;
            }
        }

        public void AddRemoveUnitActionToggled(bool isActionActive)
        {
            if (isActionActive)
            {
                return;
            }


            for(int i = 0; i < _iconButtons.Count; i++)
            {
                _iconButtons.ElementAt(i).Value.DisableButton();
            }
        }
    }
}