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
    public class UnitTabController : MonoBehaviour
    {
        [SerializeField] private UnitButton _unitButtonPrefab;
        [SerializeField] private Transform _unitButtonContainer;
        [SerializeField] private SliderWidget _setUnitTeamSlider;
        [SerializeField] private SliderWidget _addUnitTeamSlider;

        [SerializeField] private GameObject _unitDisplayPrefab;
        [SerializeField] private List<Transform> _teamPanels;
        [SerializeField] private List<Transform> _teamIndexes;

        [Header("Dependencies")]
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

            _setUnitTeamSlider.OnSliderValueChanged += OnSetUnitTeamSliderChanged;
            _addUnitTeamSlider.OnSliderValueChanged += OnAddUnitTeamSliderChanged;
        }

        private void Start()
        {
            CombatManager.Instance.OnUnitTeamChanged += CombatSystem_OnUnitTeamChanged;

            for(int i = 0; i < _teamIndexes.Count; i++)
            {
                for(int j = 0; j < _teamIndexes[i].childCount; j++)
                {
                    _teamIndexes[i].GetChild(j).GetComponent<TextMeshProUGUI>().color = CombatManager.Instance.GetTeamColor(i * _teamIndexes[i].childCount + j);
                }
            }
        }

        private void CombatSystem_OnUnitTeamChanged(Unit unit, int prevTeam, int newTeam)
        {
            for(int i = 0; i < _teamPanels.Count; i++)
            {
                for(int j = 0; j < _teamPanels[i].childCount; j++)
                {
                    Destroy(_teamPanels[i].GetChild(j).gameObject);
                }
            }
            var unitTeams = CombatManager.Instance.UnitTeams;
            foreach(var unitTeam in unitTeams)
            {
                for(int i = 0; i < unitTeam.Value.Count; i++)
                {
                    GameObject unitDisplay = Instantiate(_unitDisplayPrefab, _teamPanels[unitTeam.Key]);
                    unitDisplay.GetComponent<Image>().color = CombatManager.Instance.GetTeamColor(unitTeam.Key);
                    unitDisplay.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = unitTeam.Value.ElementAt(i).UnitData.assetData.unitIcon;
                }
            }
            //ContainerParent based on team.
            //GetComponent<Image>().Color = _teamColor;
            //unitDisplay.transform.GetChild(0).GetChild(0).GetComponent<Image>().icon = unitIcon;
        }

        private void OnSetUnitTeamSliderChanged(int sliderIndex, float value)
        {
            SetUnitTeamAction setUnitTeamAction = _playerActions.LeftClickAction.GetComponent<SetUnitTeamAction>();
            if (setUnitTeamAction)
            {
                setUnitTeamAction.UnitTeamIndex = (int)value;
            }
        }

        private void OnAddUnitTeamSliderChanged(int sliderIndex, float value)
        {
            AddUnitToGridAction addUnitAction = _playerActions.LeftClickAction.GetComponent<AddUnitToGridAction>();
            if (addUnitAction)
            {
                addUnitAction.UnitTeamIndex = (int)value;
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


            for (int i = 0; i < _iconButtons.Count; i++)
            {
                _iconButtons.ElementAt(i).Value.DisableButton();
            }
        }
    }
}