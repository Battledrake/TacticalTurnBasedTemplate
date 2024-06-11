using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatTabController : MonoBehaviour
{
    [Header("Units")]
    [SerializeField] private UnitButton _unitButtonPrefab;
    [SerializeField] private Transform _unitButtonContainer;
    [SerializeField] private Toggle _isUnitAIToggle;
    [SerializeField] private SliderWidget _setUnitTeamSlider;
    [SerializeField] private SliderWidget _addUnitTeamSlider;

    [Header("Teams")]
    [SerializeField] private GameObject _unitDisplayPrefab;
    [SerializeField] private List<Transform> _teamPanels;
    [SerializeField] private List<Transform> _teamIndexes;

    [Header("Combat")]
    [SerializeField] private TMP_Dropdown _turnOrderTypeCombo;
    [SerializeField] private Button _startCombatButton;
    [SerializeField] private TextMeshProUGUI _requirementTextHeader;
    [SerializeField] private TextMeshProUGUI _requirementTextUnits;
    [SerializeField] private TextMeshProUGUI _requirementTextTeams;
    [SerializeField] private TextMeshProUGUI _requirementTextNotInCombat;

    [Header("Dependencies")]
    [SerializeField] private PlayerActions _playerActions;

    private Dictionary<UnitId, UnitButton> _iconButtons = new Dictionary<UnitId, UnitButton>();


    private int _activeButton = -1;
    private Dictionary<Unit, GridIndex> _activeUnitsPreCombat = new Dictionary<Unit, GridIndex>();


    private void Awake()
    {
        _setUnitTeamSlider.OnSliderValueChanged += OnSetUnitTeamSliderChanged;
        _addUnitTeamSlider.OnSliderValueChanged += OnAddUnitTeamSliderChanged;
    }

    private void Start()
    {
        _isUnitAIToggle.onValueChanged.AddListener(OnUnitAIToggled);

        UnitData[] unitData = DataManager.GetAllUnitData();
        for (int i = 0; i < unitData.Length; i++)
        {
            UnitButton unitButton = Instantiate(_unitButtonPrefab, _unitButtonContainer);
            unitButton.InitializeButton(unitData[i].unitId, unitData[i].assetData.unitIcon, _playerActions);
            unitButton.OnUnitButtonToggled += OnUnitButtonToggled;

            _iconButtons.Add(unitData[i].unitId, unitButton);
        }


        for (int i = 0; i < _teamIndexes.Count; i++)
        {
            for (int j = 0; j < _teamIndexes[i].childCount; j++)
            {
                _teamIndexes[i].GetChild(j).GetComponent<TextMeshProUGUI>().color = CombatManager.Instance.GetTeamColor(i * _teamIndexes[i].childCount + j);
            }
        }
        _turnOrderTypeCombo.AddOptions(Enum.GetValues(typeof(TurnOrderType)).Cast<TurnOrderType>().Select(type => type.ToString()).ToList());
        _turnOrderTypeCombo.SetValueWithoutNotify((int)CombatManager.Instance.TurnOrderType);
        _turnOrderTypeCombo.onValueChanged.AddListener(OnTurnOrderTypeChanged);
        _startCombatButton.onClick.AddListener(OnStartCombatClicked);

        CombatManager.Instance.OnUnitTeamChanged += CombatManager_OnUnitTeamChanged;
        CombatManager.Instance.OnCombatStarted += CombatManager_OnCombatStarted;
        CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
        UpdateButtonAndTexts();
    }

    private void OnUnitAIToggled(bool isAI)
    {
        AddUnitToGridAction addUnitToGridAction = _playerActions.LeftClickAction?.GetComponent<AddUnitToGridAction>();
        if (addUnitToGridAction)
        {
            addUnitToGridAction.SetIsUsingAI(isAI);
        }
    }

    private void OnTurnOrderTypeChanged(int value)
    {
        CombatManager.Instance.TurnOrderType = (TurnOrderType)value;
    }

    private void CombatManager_OnCombatEnded()
    {
        UpdateButtonAndTexts();
        _turnOrderTypeCombo.interactable = true;
        ResetCombatUnits();
    }

    private void ResetCombatUnits()
    {
        List<Unit> unitsToCleanup = new List<Unit>();
        foreach (Unit unit in CombatManager.Instance.UnitsInCombat)
        {
            if (!_activeUnitsPreCombat.ContainsKey(unit))
            {
                unitsToCleanup.Add(unit);
            }
        }

        for (int i = 0; i < unitsToCleanup.Count; i++)
        {
            CombatManager.Instance.RemoveUnitFromCombat(unitsToCleanup[i], true, 0f);
        }

        foreach (KeyValuePair<Unit, GridIndex> unitPreCombatPair in _activeUnitsPreCombat)
        {
            unitPreCombatPair.Key.ResetUnit();
            CombatManager.Instance.TeleportUnit(unitPreCombatPair.Key, GridIndex.Invalid());
        }
        foreach (KeyValuePair<Unit, GridIndex> unitPreCombatPair in _activeUnitsPreCombat)
        {
            CombatManager.Instance.TeleportUnit(unitPreCombatPair.Key, unitPreCombatPair.Value);
        }
    }

    private void CombatManager_OnCombatStarted()
    {
        _activeUnitsPreCombat.Clear();
        List<Unit> unitsInCombat = new List<Unit>(CombatManager.Instance.UnitsInCombat);

        for (int i = 0; i < unitsInCombat.Count; i++)
        {
            _activeUnitsPreCombat.TryAdd(unitsInCombat[i], unitsInCombat[i].GetGridIndex());
        }

        _turnOrderTypeCombo.interactable = false;
    }

    private void CombatManager_OnUnitTeamChanged()
    {
        for (int i = 0; i < _teamPanels.Count; i++)
        {
            for (int j = 0; j < _teamPanels[i].childCount; j++)
            {
                Destroy(_teamPanels[i].GetChild(j).gameObject);
            }
        }
        var unitTeams = CombatManager.Instance.UnitTeams;
        foreach (KeyValuePair<int, HashSet<Unit>> unitTeam in unitTeams)
        {
            foreach (Unit unit in unitTeam.Value)
            {
                GameObject unitDisplay = Instantiate(_unitDisplayPrefab, _teamPanels[unitTeam.Key]);
                unitDisplay.GetComponent<Image>().color = CombatManager.Instance.GetTeamColor(unitTeam.Key);
                unitDisplay.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = unit.UnitData.assetData.unitIcon;
            }
        }

        UpdateButtonAndTexts();
    }

    private void OnStartCombatClicked()
    {
        CombatManager.Instance.StartCombat();

        UpdateButtonAndTexts();
    }

    [ContextMenu("CheckCombatParams")]
    public void UpdateButtonAndTexts()
    {
        CombatStartParams combatStartParams = CombatManager.Instance.CanStartCombat();
        _startCombatButton.interactable = combatStartParams.canStartCombat;
        _requirementTextHeader.color = combatStartParams.canStartCombat ? Color.green : Color.red;
        _requirementTextUnits.color = combatStartParams.hasEnoughUnits ? Color.green : Color.red;
        _requirementTextTeams.color = combatStartParams.hasEnoughTeams ? Color.green : Color.red;
        _requirementTextNotInCombat.color = combatStartParams.isNotInCombat ? Color.green : Color.red;
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

        foreach (KeyValuePair<UnitId, UnitButton> iconButtonPair in _iconButtons)
        {
            iconButtonPair.Value.DisableButton();
        }
    }

    private void OnSetUnitTeamSliderChanged(int sliderIndex, float value)
    {
        SetUnitTeamAction setUnitTeamAction = _playerActions.LeftClickAction?.GetComponent<SetUnitTeamAction>();
        if (setUnitTeamAction)
        {
            setUnitTeamAction.UnitTeamIndex = (int)value;
        }
    }

    private void OnAddUnitTeamSliderChanged(int sliderIndex, float value)
    {
        AddUnitToGridAction addUnitAction = _playerActions.LeftClickAction?.GetComponent<AddUnitToGridAction>();
        if (addUnitAction)
        {
            addUnitAction.UnitTeamIndex = (int)value;
        }
    }
}
