using BattleDrakeCreations.TTBTk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathfindingTabController : MonoBehaviour
{
    [SerializeField] private Toggle _showIndexesToggle;

    [Header("Dependencies")]
    [SerializeField] private DebugTextOnTiles _debugTextOnTiles;

    private void Awake()
    {
        _showIndexesToggle.onValueChanged.AddListener(OnShowIndexesToggled);
    }

    private void OnShowIndexesToggled(bool showIndexes)
    {
        _debugTextOnTiles.ShowTileText(showIndexes);
    }
}
