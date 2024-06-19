using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class DebugTextOnTiles : MonoBehaviour
    {
        public static DebugTextOnTiles Instance;

        [SerializeField] private TextMeshPro _debugTextPrefab;
        [SerializeField] private Transform _instanceContainer;
        [SerializeField] private int _initialPoolCount = 1000;

        [Header("Dependencies")]
        [SerializeField] private TacticsGrid _tacticsGrid;

        public bool ShowTileIndexes { get => _showTileIndexes; set => _showTileIndexes = value; }
        public bool ShowUnitOnTile { get => _showUnitOnTile; set => _showUnitOnTile = value; }
        public bool ShowTerrainCost { get => _showTerrainCost; set => _showTerrainCost = value; }
        public bool ShowTraversalCost { get => _showTraversalCost; set => _showTraversalCost = value; }
        public bool ShowHeuristicCost { get => _showHeuristicCost; set => _showHeuristicCost = value; }
        public bool ShowTotalCost { get => _showTotalCost; set => _showTotalCost = value; }
        public bool ShowClimbLinks { get => _showClimbLinks; set => _showClimbLinks = value; }
        public bool ShowCover { get => _showCover; set => _showCover = value; }

        private Dictionary<GridIndex, TextMeshPro> _activeDebugTexts = new Dictionary<GridIndex, TextMeshPro>();
        private List<TextMeshPro> _pooledTextInstances = new();

        private bool _showTileIndexes = false;
        private bool _showUnitOnTile = false;
        private bool _showTerrainCost = false;
        private bool _showTraversalCost = false;
        private bool _showHeuristicCost = false;
        private bool _showTotalCost = false;
        private bool _showClimbLinks = false;
        private bool _showCover = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this.gameObject);

            for (int i = 0; i < _initialPoolCount; i++)
            {
                TextMeshPro newTextInstance = Instantiate(_debugTextPrefab, _instanceContainer);
                _pooledTextInstances.Add(newTextInstance);
                newTextInstance.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _tacticsGrid.OnGridGenerated += TacticsGrid_OnGridGenerated;
            _tacticsGrid.OnTileDataUpdated += (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed += TacticsGrid_OnGridDestroyed;
            _tacticsGrid.Pathfinder.OnPathfindingCompleted += UpdateTextOnAllTiles;
            _tacticsGrid.Pathfinder.OnPathfindingDataCleared += UpdateTextOnAllTiles;
            _tacticsGrid.Pathfinder.OnPathfindingDataUpdated += UpdateTextOnAllTiles;
        }

        private void TacticsGrid_OnGridGenerated()
        {
            if (_tacticsGrid.GridTiles.Count > _pooledTextInstances.Count)
            {
                int gridCount = _tacticsGrid.GridTiles.Count;
                int pooledCount = _pooledTextInstances.Count;
                int difference = gridCount - pooledCount;
                for (int i = 0; i < difference; i++)
                {
                    TextMeshPro newObject = Instantiate(_debugTextPrefab, _instanceContainer);
                    _pooledTextInstances.Add(newObject);
                }
            }

            for (int i = 0; i < _pooledTextInstances.Count; i++)
            {
                _pooledTextInstances[i].gameObject.SetActive(false);
            }

            _activeDebugTexts.Clear();

            int itemCount = 0;
            foreach (KeyValuePair<GridIndex, TileData> gridTilePair in _tacticsGrid.GridTiles)
            {
                _activeDebugTexts.TryAdd(gridTilePair.Key, _pooledTextInstances[itemCount]);
                itemCount++;
            }
            UpdateTextOnAllTiles();
        }

        private void TacticsGrid_OnGridDestroyed()
        {
            for (int i = 0; i < _pooledTextInstances.Count; i++)
            {
                _pooledTextInstances[i].gameObject.SetActive(false);
            }

            _activeDebugTexts.Clear();
        }

        private void OnDisable()
        {
            _tacticsGrid.OnGridGenerated -= UpdateTextOnAllTiles;
            _tacticsGrid.OnTileDataUpdated -= (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed -= HideAllDebugTiles;
            _tacticsGrid.Pathfinder.OnPathfindingCompleted -= UpdateTextOnAllTiles;
            _tacticsGrid.Pathfinder.OnPathfindingDataCleared -= UpdateTextOnAllTiles;
            _tacticsGrid.Pathfinder.OnPathfindingDataUpdated -= UpdateTextOnAllTiles;
        }

        private bool ShowAnyDebug()
        {
            return _showTileIndexes || _showTerrainCost || _showUnitOnTile || _showClimbLinks || _showCover || HasPathfindingData();
        }

        private bool HasPathfindingData()
        {
            return _showTraversalCost || _showHeuristicCost || _showTotalCost;
        }

        private void UpdateTextOnAllTiles()
        {
            if (!ShowAnyDebug())
                return;

            foreach (KeyValuePair<GridIndex, TileData> gridTilePair in _tacticsGrid.GridTiles)
            {
                UpdateTextOnTile(gridTilePair.Key);
            }
        }

        private void UpdateTextOnTile(GridIndex index)
        {
            bool isValidTile = _tacticsGrid.GridTiles.TryGetValue(index, out TileData tileData);
            if (ShowAnyDebug() && isValidTile && GridStatics.IsTileTypeTraversable(tileData.tileType))
            {
                string debugText = "";

                TextMeshPro debugTile = GetDebugTile(index);
                if (debugTile == null)
                    return;

                if (_tacticsGrid.GridShape == GridShape.Triangle)
                    debugTile.fontSize = 0.75f;
                if (_tacticsGrid.GridShape == GridShape.Hexagon)
                    debugTile.fontSize = 1f;

                if (_showTileIndexes)
                    debugText += $"index: {index}\n";

                if (_showUnitOnTile)
                {
                    string unitText = tileData.unitOnTile ? tileData.unitOnTile.name : "none";
                    debugText += string.Format("unit:{0}\n", unitText);
                }

                if (_showTerrainCost)
                    debugText += string.Format("terrain:{0:F1}\n", PathfindingStatics.GetTerrainCostFromTileType(tileData.tileType));

                if (HasPathfindingData())
                {
                    if (_tacticsGrid.Pathfinder.PathNodePool != null)
                    {
                        if (_tacticsGrid.Pathfinder.PathNodePool.TryGetValue(index, out PathNode pathNode))
                        {

                            if (_showTraversalCost && pathNode.traversalCost != Mathf.Infinity)
                                debugText += string.Format("traversal:{0:F1}\n", pathNode.traversalCost);

                            if (_showHeuristicCost && pathNode.heuristicCost != Mathf.Infinity)
                                debugText += string.Format("heuristic:{0:F1}\n", pathNode.heuristicCost);

                            if (_showTotalCost && pathNode.totalCost != Mathf.Infinity)
                                debugText += string.Format("total:{0:F1}\n", pathNode.totalCost);
                        }
                    }
                }

                if (_showClimbLinks)
                {
                    if (tileData.climbData.climbLinks != null && tileData.climbData.climbLinks.Count > 0)
                    {
                        string climbText = "";
                        for (int i = 0; i < tileData.climbData.climbLinks.Count; i++)
                        {
                            climbText += "cl:" + tileData.climbData.climbLinks[i] + "\n";
                        }
                        debugText += climbText;
                    }
                }

                if (_showCover)
                {
                    if (tileData.cover.hasCover)
                    {
                        debugText += "covers: \n";
                        for(int i = 0; i < tileData.cover.data.Count; i++)
                        {
                            CoverType coverType = tileData.cover.data[i].coverType;
                            string directionCharacter = CharacterFromDirection(tileData.cover.data[i].direction);
                            //character = coverType == CoverType.HalfCover ? character.ToLower() : character.ToUpper();
                            debugText += $"({directionCharacter + CharacterFromCoverType(tileData.cover.data[i].coverType)})";
                            //debugText += $"({character}: �)"; //� alt + 0166
                        }
                    }
                }

                if (string.IsNullOrEmpty(debugText))
                {
                    //DestroyTextGameObject(index);
                    return;
                }

                debugTile.text = debugText;

                Vector3 tilePosition = tileData.tileMatrix.GetPosition();
                tilePosition.y += 0.1f;
                debugTile.transform.position = tilePosition;
                debugTile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                debugTile.transform.localScale = tileData.tileMatrix.lossyScale;

                debugTile.gameObject.SetActive(true);
            }
            else
            {

                if (_activeDebugTexts.TryGetValue(index, out TextMeshPro tile))
                {
                    tile.gameObject.SetActive(false);
                }
            }
        }

        private TextMeshPro GetDebugTile(GridIndex index)
        {
            if (_activeDebugTexts.ContainsKey(index))
            {
                return _activeDebugTexts[index];
            }
            return null;
        }

        public void UpdateDebugText()
        {
            if (!ShowAnyDebug())
            {
                HideAllDebugTiles();
            }
            else
            {
                foreach (KeyValuePair<GridIndex, TileData> gridTilePair in _tacticsGrid.GridTiles)
                {
                    UpdateTextOnTile(gridTilePair.Key);
                }
            }
        }

        private void HideAllDebugTiles()
        {
            foreach (KeyValuePair<GridIndex, TextMeshPro> debugTilePair in _activeDebugTexts)
            {
                debugTilePair.Value.gameObject.SetActive(false);
            }
        }

        private string CharacterFromDirection(GridIndex direction)
        {
            if (direction == new GridIndex(0, 1))
                return "n";
            if (direction == new GridIndex(1, 0))
                return "e";
            if (direction == new GridIndex(0, -1))
                return "s";
            if (direction == new GridIndex(-1, 0))
                return "w";
            return "";
        }

        private string CharacterFromCoverType(CoverType coverType)
        {
            switch (coverType)
            {
                case CoverType.None:
                    break;
                case CoverType.HalfCover:
                    return "-"; //alt + 0189 for � 
                case CoverType.FullCover:
                    return "=";
            }
            return "";
        }
    }
}
