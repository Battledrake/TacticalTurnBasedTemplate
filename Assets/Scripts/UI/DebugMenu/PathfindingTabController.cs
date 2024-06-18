using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class PathfindingTabController : MonoBehaviour
    {
        [Header("Debug Toggles")]
        [SerializeField] private Toggle _showIndexesToggle;
        [SerializeField] private Toggle _showUnitToggle;
        [SerializeField] private Toggle _showTerrainCostToggle;
        [SerializeField] private Toggle _showTraversalCostToggle;
        [SerializeField] private Toggle _showHeuristicToggle;
        [SerializeField] private Toggle _showTotalCostToggle;
        [SerializeField] private Toggle _showClimbLinksToggle;

        [Header("Configuration")]
        [SerializeField] private SliderWidget _heightAllowanceSlider;
        [SerializeField] private TMP_Dropdown _traversalCostCombo;
        [SerializeField] private TMP_Dropdown _heuristicCostCombo;
        [SerializeField] private TMP_Dropdown _traversalTypeCombo;
        [SerializeField] private SliderWidget _heuristicScaleSlider;
        [SerializeField] private Toggle _includeDiagonalsToggle;
        [SerializeField] private Toggle _allowPartialSolutionsToggle;
        [SerializeField] private Toggle _ignoreClosedToggle;
        [SerializeField] private Toggle _includeStartNodeToggle;

        [Header("Dependencies")]
        [SerializeField] private GridPathfinding _gridPathfinder;

        private void Awake()
        {
            _showIndexesToggle.onValueChanged.AddListener(OnShowIndexesToggled);
            _showUnitToggle.onValueChanged.AddListener(OnShowUnitToggled);
            _showTerrainCostToggle.onValueChanged.AddListener(OnShowTerrainCostToggled);
            _showTraversalCostToggle.onValueChanged.AddListener(OnShowTraversalCostToggled);
            _showHeuristicToggle.onValueChanged.AddListener(OnShowHeuristicToggled);
            _showTotalCostToggle.onValueChanged.AddListener(OnShowTotalCostToggled);
            _showClimbLinksToggle.onValueChanged.AddListener(OnShowClimbLinksToggled);

            _heightAllowanceSlider.SetSliderValueWithoutNotify(_gridPathfinder.HeightAllowance);
            _traversalCostCombo.SetValueWithoutNotify((int)_gridPathfinder.TraversalCost);
            _heuristicCostCombo.SetValueWithoutNotify((int)_gridPathfinder.HeuristicCost);
            _traversalTypeCombo.SetValueWithoutNotify((int)_gridPathfinder.SquareTraversalType);
            _heuristicScaleSlider.SetSliderValueWithoutNotify(_gridPathfinder.HeuristicScale);
            _includeDiagonalsToggle.SetIsOnWithoutNotify(_gridPathfinder.IncludeDiagonals);
            _allowPartialSolutionsToggle.SetIsOnWithoutNotify(_gridPathfinder.AllowPartialSolution);
            _ignoreClosedToggle.SetIsOnWithoutNotify(_gridPathfinder.IgnoreClosed);
            _includeStartNodeToggle.SetIsOnWithoutNotify(_gridPathfinder.IncludeStartNodeInPath);

            _heightAllowanceSlider.OnSliderValueChanged += OnHeightAllowanceChanged;
            _traversalCostCombo.onValueChanged.AddListener(OnTraversalCostComboChanged);
            _heuristicCostCombo.onValueChanged.AddListener(OnHeuristicCostComboChanged);
            _traversalTypeCombo.onValueChanged.AddListener(OnTraversalTypeComboChanged);
            _heuristicScaleSlider.OnSliderValueChanged += OnHeuristicScaleChanged;
            _includeDiagonalsToggle.onValueChanged.AddListener(OnIncludeDiagonalsTogged);
            _allowPartialSolutionsToggle.onValueChanged.AddListener(OnAllowPartialSolutionsToggled);
            _ignoreClosedToggle.onValueChanged.AddListener(OnIgnoreClosedToggled);
            _includeStartNodeToggle.onValueChanged.AddListener(OnIncludeStartNodeToggled);
        }

        private void OnShowClimbLinksToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowClimbLinks = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowIndexesToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowTileIndexes = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowUnitToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowUnitOnTile = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }
        private void OnShowTerrainCostToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowTerrainCost = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowTraversalCostToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowTraversalCost = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowHeuristicToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowHeuristicCost = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowTotalCostToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowTotalCost = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnHeightAllowanceChanged(int sliderIndex, float value)
        {
            _gridPathfinder.HeightAllowance = value;
        }

        private void OnTraversalCostComboChanged(int newValue)
        {
            _gridPathfinder.TraversalCost = (CalculationType)newValue;
        }

        private void OnHeuristicCostComboChanged(int newValue)
        {
            _gridPathfinder.HeuristicCost = (CalculationType)newValue;
        }

        private void OnTraversalTypeComboChanged(int newValue)
        {
            _gridPathfinder.SquareTraversalType = (TraversalType)newValue;
        }

        private void OnHeuristicScaleChanged(int sliderIndex, float value)
        {
            _gridPathfinder.HeuristicScale = value;

        }

        private void OnIncludeDiagonalsTogged(bool isOn)
        {
            _gridPathfinder.IncludeDiagonals = isOn;
        }

        private void OnAllowPartialSolutionsToggled(bool isOn)
        {
            _gridPathfinder.AllowPartialSolution = isOn;
        }
        private void OnIgnoreClosedToggled(bool isOn)
        {
            _gridPathfinder.IgnoreClosed = isOn;
        }
        private void OnIncludeStartNodeToggled(bool isOn)
        {
            _gridPathfinder.IncludeStartNodeInPath = isOn;
        }
    }
}