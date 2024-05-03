using BattleDrakeCreations.TTBTk;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridTabController : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private TMP_Dropdown _sceneCombo;

    [Header("Grid Generation")]
    [SerializeField] private TMP_Dropdown _gridShapeCombo;
    [SerializeField] private SliderWidget _locationSlider;
    [SerializeField] private SliderWidget _tileCountSlider;
    [SerializeField] private SliderWidget _tileSizeSlider;
    [SerializeField] private SliderWidget _groundOffsetSlider;
    [SerializeField] private Toggle _useEnvToggle;

    [Header("Debug")]
    [SerializeField] private Toggle _boundsToggle;
    [SerializeField] private Toggle _centerToggle;
    [SerializeField] private Toggle _bottomLeftToggle;

    [Header("Dependencies")]
    [SerializeField] private SceneLoading _sceneLoader;
    [SerializeField] private TacticsGrid _tacticsGrid;

    private void Awake()
    {
        _gridShapeCombo.value = (int)_tacticsGrid.GridShape;
        _locationSlider.SetSliderValue(_tacticsGrid.transform.position);
        _tileCountSlider.SetSliderValue(_tacticsGrid.GridTileCount);
        _tileSizeSlider.SetSliderValue(_tacticsGrid.TileSize);
        _boundsToggle.SetIsOnWithoutNotify(_tacticsGrid.ShowDebugLines);
        _centerToggle.SetIsOnWithoutNotify(_tacticsGrid.ShowDebugCenter);
        _bottomLeftToggle.SetIsOnWithoutNotify(_tacticsGrid.ShowDebugStart);
        _groundOffsetSlider.SetSliderValue(_tacticsGrid.GroundOffset);
        _useEnvToggle.SetIsOnWithoutNotify(_tacticsGrid.UseEnvironment);

        _sceneCombo.onValueChanged.AddListener(OnSceneChanged);
        _gridShapeCombo.onValueChanged.AddListener(OnGridShapeChanged);
        _locationSlider.OnSliderValueChanged += OnLocationChanged;
        _tileCountSlider.OnSliderValueChanged += OnTileCountChanged;
        _tileSizeSlider.OnSliderValueChanged += OnTileSizeChanged;
        _groundOffsetSlider.OnSliderValueChanged += OnGroundOffsetChanged;
        _useEnvToggle.onValueChanged.AddListener(OnUseEnvironmentChanged);


        _boundsToggle.onValueChanged.AddListener(OnBoundsToggle);
        _centerToggle.onValueChanged.AddListener(OnCenterToggle);
        _bottomLeftToggle.onValueChanged.AddListener(OnBottomLeftToggle);
    }

    private void OnUseEnvironmentChanged(bool useEnvironment)
    {
        _tacticsGrid.UseEnvironment = useEnvironment;

        _tacticsGrid.RespawnGrid();
    }

    private void OnSceneChanged(int index)
    {
        if (index > 0)
        {
            _sceneLoader.LoadScene(_sceneCombo.options[index].text);
        }
        else
        {
            _sceneLoader.UnloadScene();
        }
    }

    private void OnGridShapeChanged(int gridShape)
    {
        _tacticsGrid.GridShape = (GridShape)gridShape;

        _tacticsGrid.RespawnGrid();
    }

    private void OnLocationChanged(int sliderIndex, float value)
    {
        Vector3 newVector = _tacticsGrid.transform.position;
        switch (sliderIndex)
        {
            case 0: //x
                newVector.x = value;
                break;
            case 1: //y
                newVector.y = value;
                break;
            case 2: //z
                newVector.z = value;
                break;
            default:
                break;
        }
        _tacticsGrid.transform.position = newVector;
    }

    private void OnTileCountChanged(int sliderIndex, float value)
    {
        Vector2Int tileCount = _tacticsGrid.GridTileCount;
        if (sliderIndex == 0)
            tileCount.x = Mathf.RoundToInt(value);
        else if (sliderIndex == 1)
            tileCount.y = Mathf.RoundToInt(value);

        _tacticsGrid.GridTileCount = tileCount;

        _tacticsGrid.RespawnGrid();
    }

    private void OnTileSizeChanged(int sliderIndex, float value)
    {
        Vector3 tileSize = _tacticsGrid.TileSize;
        switch (sliderIndex)
        {
            case 0: //x
                tileSize.x = value;
                break;
            case 1: //y
                tileSize.y = value;
                break;
            case 2: //z
                tileSize.z = value;
                break;
            default:
                break;
        }
        _tacticsGrid.TileSize = tileSize;

        _tacticsGrid.RespawnGrid();
    }

    private void OnGroundOffsetChanged(int index, float value)
    {
        _tacticsGrid.GroundOffset = value;

        _tacticsGrid.RespawnGrid();
    }

    private void OnBoundsToggle(bool showBounds)
    {
        _tacticsGrid.ShowDebugLines = showBounds;
    }

    private void OnCenterToggle(bool showCenter)
    {
        _tacticsGrid.ShowDebugCenter = showCenter;
    }

    private void OnBottomLeftToggle(bool showBottomLeft)
    {
        _tacticsGrid.ShowDebugStart = showBottomLeft;
    }
}
