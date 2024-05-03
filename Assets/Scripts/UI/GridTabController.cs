using BattleDrakeCreations.TTBTk;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridTabController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _gridShapeCombo;
    [SerializeField] private SliderWidget _locationSlider;
    [SerializeField] private SliderWidget _tileCountSlider;
    [SerializeField] private SliderWidget _tileSizeSlider;
    [SerializeField] private Toggle _boundsToggle;
    [SerializeField] private Toggle _centerToggle;
    [SerializeField] private Toggle _bottomLeftToggle;

    [SerializeField] private TacticsGrid _tacticsGrid;

    private void Awake()
    {
        _gridShapeCombo.value = (int)_tacticsGrid.GridShape;
        _locationSlider.SetSliderValue(_tacticsGrid.transform.position);
        _tileCountSlider.SetSliderValue(new Vector2(_tacticsGrid.GridWidth, _tacticsGrid.GridHeight));
        _tileSizeSlider.SetSliderValue(_tacticsGrid.TileSize);
        _boundsToggle.SetIsOnWithoutNotify(_tacticsGrid.ShowDebugLines);
        _centerToggle.SetIsOnWithoutNotify(_tacticsGrid.ShowDebugCenter);
        _bottomLeftToggle.SetIsOnWithoutNotify(_tacticsGrid.ShowDebugStart);

        _gridShapeCombo.onValueChanged.AddListener(OnGridShapeChanged);
        _locationSlider.OnSliderValueChanged += OnLocationChanged;
        _tileCountSlider.OnSliderValueChanged += OnTileCountChanged;
        _tileSizeSlider.OnSliderValueChanged += OnTileSizeChanged;
        _boundsToggle.onValueChanged.AddListener(OnBoundsToggle);
        _centerToggle.onValueChanged.AddListener(OnCenterToggle);
        _bottomLeftToggle.onValueChanged.AddListener(OnBottomLeftToggle);
    }

    private void OnGridShapeChanged(int gridShape)
    {
        _tacticsGrid.GridShape = (GridShape)gridShape;
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
        if (sliderIndex == 0)
            _tacticsGrid.GridWidth = Mathf.RoundToInt(value);
        else if (sliderIndex == 1)
            _tacticsGrid.GridHeight = Mathf.RoundToInt(value);
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
