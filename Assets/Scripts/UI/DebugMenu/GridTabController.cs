using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class GridTabController : MonoBehaviour
    {
        [Header("Actions")]
        [SerializeField] private TMP_Dropdown _tileTypeCombo;

        [Header("Environment")]
        [SerializeField] private TMP_Dropdown _sceneCombo;

        [Header("Grid Generation")]
        [SerializeField] private CanvasGroup _gridGenCanvasGroup;
        [SerializeField] private TMP_Dropdown _gridShapeCombo;
        [SerializeField] private SliderWidget _positionSlider;
        [SerializeField] private SliderWidget _tileCountSlider;
        [SerializeField] private SliderWidget _tileSizeSlider;
        [SerializeField] private SliderWidget _groundOffsetSlider;
        [SerializeField] private Toggle _useEnvToggle;

        [Header("Debug Tile Toggles")]
        [SerializeField] private Toggle _showGridToggle;
        [SerializeField] private Toggle _tacticalMeshGridToggle;
        [SerializeField] private Toggle _hideGridModifiersToggle;
        [SerializeField] private Toggle _showIndexesToggle;
        [SerializeField] private Toggle _showUnitOnTileToggle;
        [SerializeField] private Toggle _showClimbLinksToggle;
        [SerializeField] private Toggle _showCoversToggle;

        [Header("Gizmo Toggles")]
        [SerializeField] private Toggle _boundsToggle;
        [SerializeField] private Toggle _centerToggle;
        [SerializeField] private TextMeshProUGUI _centerPositionText;
        [SerializeField] private Toggle _bottomLeftToggle;
        [SerializeField] private TextMeshProUGUI _bottomLeftText;
        [SerializeField] private Toggle _mousePositionToggle;
        [SerializeField] private TextMeshProUGUI _mousePositionText;
        [SerializeField] private Toggle _hoveredTileToggle;
        [SerializeField] private TextMeshProUGUI _hoveredTileText;

        [Header("Dependencies")]
        [SerializeField] private SceneLoading _sceneLoader;
        [SerializeField] private TacticsGrid _tacticsGrid;

        private int _sceneSelected = 0;
        private List<GameObject> _environmentObjects = new List<GameObject>();
        private List<GameObject> _modifierObjects = new List<GameObject>();

        private bool _showGridBounds = false;
        private bool _showGridCenter = false;
        private bool _showGridBottomLeft = false;
        private bool _showMousePosition = false;
        private bool _showHoveredTile = false;

        private void Awake()
        {
            _tileTypeCombo.ClearOptions();
            _tileTypeCombo.AddOptions(Enum.GetValues(typeof(TileType)).Cast<TileType>().Select(type => type.ToString()).ToList());

            List<string> buildScenes = new List<string>();
            buildScenes.Add("None");
            int sceneCount = _sceneLoader.ScenePool.Count;
            _sceneCombo.ClearOptions();
            for (int i = 0; i < sceneCount; i++)
            {
                buildScenes.Add(_sceneLoader.ScenePool[i].name);
            }
            _sceneCombo.AddOptions(buildScenes);

            _sceneCombo.onValueChanged.AddListener(OnSceneChanged);
            _gridShapeCombo.onValueChanged.AddListener(OnGridShapeChanged);
            _positionSlider.OnSliderValueChanged += OnGridPositionChanged;
            _tileCountSlider.OnSliderValueChanged += OnTileCountChanged;
            _tileSizeSlider.OnSliderValueChanged += OnTileSizeChanged;
            _groundOffsetSlider.OnSliderValueChanged += OnGroundOffsetChanged;
            _useEnvToggle.onValueChanged.AddListener(OnUseEnvironmentToggled);

            _showGridToggle.onValueChanged.AddListener(OnShowGridToggled);
            _tacticalMeshGridToggle.onValueChanged.AddListener(OnTacticalMeshGridToggled);
            _hideGridModifiersToggle.onValueChanged.AddListener(OnHideGridModifiersToggled);

            _showIndexesToggle.onValueChanged.AddListener(OnShowIndexesToggled);
            _showUnitOnTileToggle.onValueChanged.AddListener(OnShowUnitOnTileToggled);
            _showClimbLinksToggle.onValueChanged.AddListener(OnShowClimbLinksToggled);
            _showCoversToggle.onValueChanged.AddListener(OnShowCoversToggled);

            _boundsToggle.onValueChanged.AddListener(OnBoundsToggled);
            _centerToggle.onValueChanged.AddListener(OnCenterToggled);
            _bottomLeftToggle.onValueChanged.AddListener(OnBottomLeftToggled);
            _mousePositionToggle.onValueChanged.AddListener(OnMousePositionToggled);
            _hoveredTileToggle.onValueChanged.AddListener(OnHoveredTileToggled);
        }

        private void OnShowCoversToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowCover = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowClimbLinksToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowClimbLinks = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowUnitOnTileToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowUnitOnTile = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void OnShowIndexesToggled(bool isOn)
        {
            DebugTextOnTiles.Instance.ShowTileIndexes = isOn;
            DebugTextOnTiles.Instance.UpdateDebugText();
        }

        private void Start()
        {
            _gridShapeCombo.value = (int)_tacticsGrid.GridShape;
            _positionSlider.SetSliderValueWithoutNotify(_tacticsGrid.transform.position);
            _tileCountSlider.SetSliderValueWithoutNotify(_tacticsGrid.GridTileCount);
            _tileSizeSlider.SetSliderValueWithoutNotify(_tacticsGrid.TileSize);
            _groundOffsetSlider.SetSliderValueWithoutNotify(_tacticsGrid.GridVisual.GroundOffset);
            _useEnvToggle.SetIsOnWithoutNotify(_tacticsGrid.UseEnvironment);
            _showGridToggle.SetIsOnWithoutNotify(_tacticsGrid.GridVisual.GridMeshInstancer.ShowBaseGrid);
            _tacticalMeshGridToggle.SetIsOnWithoutNotify(false);
        }

        private void OnEnable()
        {
            CombatManager.Instance.OnCombatStarted += () => _gridGenCanvasGroup.interactable = false;
            CombatManager.Instance.OnCombatEnded += () => _gridGenCanvasGroup.interactable = true;
        }

        private void Update()
        {
            if (_showGridBounds)
            {
                Bounds gridBounds = _tacticsGrid.GetGridBounds();
                DebugExtension.DebugBounds(gridBounds, Color.yellow);
            }
            if (_showGridCenter)
            {
                Vector3 gridCenter = _tacticsGrid.GetGridCenterPosition();
                DebugExtension.DebugWireSphere(gridCenter, Color.yellow, 0.1f);
            }
            if (_showGridBottomLeft)
            {
                Vector3 bottomLeftPosition = _tacticsGrid.transform.position;
                _bottomLeftText.text = bottomLeftPosition.ToString("F1");
                bottomLeftPosition.y += 0.1f;
                DebugExtension.DebugWireSphere(bottomLeftPosition, Color.yellow, 0.1f);
            }
            if (_showMousePosition)
            {
                Vector3 mousePosition = _tacticsGrid.GetCursorPositionOnGrid();
                _mousePositionText.text = mousePosition.ToString("F1");
                DebugExtension.DebugWireSphere(mousePosition, Color.yellow, 0.1f);
            }
            if (_showHoveredTile)
            {
                GridIndex hoveredTileIndex = _tacticsGrid.GetTileIndexUnderCursor();
                _hoveredTileText.text = hoveredTileIndex.ToString();

                if (_tacticsGrid.GridTiles.TryGetValue(hoveredTileIndex, out TileData tileData))
                {
                    DebugExtension.DebugBounds(new Bounds(tileData.tileMatrix.GetPosition(), new Vector3(.5f, .5f, .5f)), Color.yellow);
                }
            }
        }

        private void OnSceneChanged(int index)
        {
            if (index > 0)
            {
                _sceneLoader.UnloadScene(_sceneCombo.options[_sceneSelected].text);
                _sceneLoader.LoadScene(_sceneCombo.options[index].text);
                _sceneSelected = index;
            }
            else
            {
                if (_sceneSelected == 0)
                    return;

                _sceneLoader.UnloadScene(_sceneCombo.options[_sceneSelected].text);
                _sceneSelected = 0;
            }
        }

        private void OnGridShapeChanged(int gridShape)
        {
            _tacticsGrid.GridShape = (GridShape)gridShape;

            _tacticsGrid.RespawnGrid();
        }

        private void OnGridPositionChanged(int sliderIndex, float value)
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
            GridIndex tileCount = _tacticsGrid.GridTileCount;
            if (sliderIndex == 0)
                tileCount.x = Mathf.RoundToInt(value);
            else if (sliderIndex == 1)
                tileCount.z = Mathf.RoundToInt(value);

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
            _tacticsGrid.GridVisual.GroundOffset = value;

            _tacticsGrid.RespawnGrid();
        }

        private void OnUseEnvironmentToggled(bool useEnvironment)
        {
            _tacticsGrid.UseEnvironment = useEnvironment;

            _tacticsGrid.RespawnGrid();
        }

        private void OnShowGridToggled(bool showGrid)
        {
            _tacticsGrid.GridVisual.GridMeshInstancer.ShowBaseGrid = showGrid;
        }

        private void OnTacticalMeshGridToggled(bool isOn)
        {
            if (isOn)
            {
                //if (_sceneSelected > 0)
                //    _sceneLoader.UnloadScene(_sceneCombo.options[_sceneSelected].text);

                _environmentObjects = GameObject.FindGameObjectsWithTag("Environment").ToList();

                if (_useEnvToggle.isOn)
                {
                    foreach (GameObject obj in _environmentObjects)
                    {
                        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(false);
                        foreach (MeshRenderer render in renderers)
                            render.enabled = false;
                    }
                }
                else
                {
                    _environmentObjects.ForEach(o => o.SetActive(false));
                }

                _tacticsGrid.GridVisual.HideDefaultGrid();
                _tacticsGrid.GridVisual.ShowTacticalGrid();

            }
            else
            {
                //if (_sceneSelected > 0)
                //    _sceneLoader.LoadScene(_sceneCombo.options[_sceneSelected].text);

                foreach (GameObject obj in _environmentObjects)
                {
                    MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (MeshRenderer render in renderers)
                        render.enabled = true;
                }
                _environmentObjects.ForEach(o => o.SetActive(true));

                _tacticsGrid.GridVisual.HideTacticalGrid();
            }
        }

        private void OnHideGridModifiersToggled(bool isOn)
        {
            if (isOn)
            {
                _modifierObjects = GameObject.FindGameObjectsWithTag("GridModifier").ToList();

                foreach (GameObject obj in _modifierObjects)
                {
                    MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(false);
                    foreach (MeshRenderer render in renderers)
                        render.enabled = false;
                }
            }
            else
            {

                foreach (GameObject obj in _modifierObjects)
                {
                    MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (MeshRenderer render in renderers)
                        render.enabled = true;
                }
            }
        }

        private void OnBoundsToggled(bool showGridBounds)
        {
            _showGridBounds = showGridBounds;
        }

        private void OnCenterToggled(bool showGridCenter)
        {
            _showGridCenter = showGridCenter;
        }

        private void OnBottomLeftToggled(bool showBottomLeft)
        {
            _showGridBottomLeft = showBottomLeft;
        }

        private void OnMousePositionToggled(bool showMousePosition)
        {
            _showMousePosition = showMousePosition;
        }

        private void OnHoveredTileToggled(bool showHoveredTile)
        {
            _showHoveredTile = showHoveredTile;
        }
    }
}
