using BattleDrakeCreations.TTBTk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathfindingTabController : MonoBehaviour
{
    [Header("Debug Toggles")]
    [SerializeField] private Toggle _showIndexesToggle;
    [SerializeField] private Toggle _showTraversalCostToggle;
    [SerializeField] private Toggle _showHeuristicToggle;
    [SerializeField] private Toggle _showTotalCostToggle;

    [Header("Dependencies")]
    [SerializeField] private DebugTextOnTiles _debugTextOnTiles;

    private void Awake()
    {
        _showIndexesToggle.onValueChanged.AddListener(OnShowIndexesToggled);
        _showTraversalCostToggle.onValueChanged.AddListener(OnShowTraversalCostToggled);
        _showHeuristicToggle.onValueChanged.AddListener(OnShowHeuristicToggled);
        _showTotalCostToggle.onValueChanged.AddListener(OnShowTotalCostToggled);
    }

    private void OnShowIndexesToggled(bool isOn)
    {
        _debugTextOnTiles.ShowTileIndexes = isOn;
        _debugTextOnTiles.UpdateDebugText();
    }

    private void OnShowTraversalCostToggled(bool isOn)
    {
        _debugTextOnTiles.ShowTraversalCost = isOn;
        _debugTextOnTiles.UpdateDebugText();
    }

    private void OnShowHeuristicToggled(bool isOn)
    {
        _debugTextOnTiles.ShowHeuristicCost = isOn;
        _debugTextOnTiles.UpdateDebugText();
    }

    private void OnShowTotalCostToggled(bool isOn)
    {
        _debugTextOnTiles.ShowTotalCost = isOn;
        _debugTextOnTiles.UpdateDebugText();
    }
}
