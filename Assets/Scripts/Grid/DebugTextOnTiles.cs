using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class DebugTextOnTiles : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textObjectPrefab;

        [Header("Dependencies")]
        [SerializeField] private TacticsGrid _tacticsGrid;

        public bool ShowTileIndexes { get => _showTileIndexes; set => _showTileIndexes = value; }
        public bool ShowTerrainCost { get => _showTerrainCost; set => _showTerrainCost = value; }
        public bool ShowTraversalCost { get => _showTraversalCost; set => _showTraversalCost = value; }
        public bool ShowHeuristicCost { get => _showHeuristicCost; set => _showHeuristicCost = value; }
        public bool ShowTotalCost { get => _showTotalCost; set => _showTotalCost = value; }

        private Dictionary<GridIndex, GameObject> _spawnedTexts = new Dictionary<GridIndex, GameObject>();

        private bool _showDebugText = false;
        private bool _showTileIndexes = false;
        private bool _showTerrainCost = false;
        private bool _showTraversalCost = false;
        private bool _showHeuristicCost = false;
        private bool _showTotalCost = false;

        private void OnEnable()
        {
            _tacticsGrid.OnTileDataUpdated += (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed += ClearAllTextGameObjects;
            _tacticsGrid.GridPathfinder.OnPathfindingDataUpdated += UpdateTextOnTile;
            _tacticsGrid.GridPathfinder.OnPathfindingDataCleared += UpdateTextOnAllTiles;
        }

        private void OnDisable()
        {
            _tacticsGrid.OnTileDataUpdated -= (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed -= ClearAllTextGameObjects;
            _tacticsGrid.GridPathfinder.OnPathfindingDataUpdated -= UpdateTextOnTile;
            _tacticsGrid.GridPathfinder.OnPathfindingDataCleared -= UpdateTextOnAllTiles;
        }

        private bool ShowAnyDebug()
        {
            return _showTileIndexes || NeedPathfindingData();
        }

        private bool NeedPathfindingData()
        {
            return _showTerrainCost || _showTraversalCost || _showHeuristicCost || _showTotalCost;
        }

        public void UpdateTextOnAllTiles()
        {
            for(int i = 0; i < _tacticsGrid.GridTiles.Count; i++)
            {
                UpdateTextOnTile(_tacticsGrid.GridTiles.ElementAt(i).Key);
            }
        }

        public void UpdateTextOnTile(GridIndex index)
        {
            if (_showDebugText && _tacticsGrid.GridTiles.TryGetValue(index, out TileData tileData) && GridStatics.IsTileTypeWalkable(tileData.tileType))
            {
                string debugText = "";

                TextMeshPro textObject = GetTextGameObject(index)?.GetComponent<TextMeshPro>();

                if (_tacticsGrid.GridShape == GridShape.Triangle)
                    textObject.fontSize = 0.75f;
                if (_tacticsGrid.GridShape == GridShape.Hexagon)
                    textObject.fontSize = 1f;

                if (_showTileIndexes)
                    debugText += $"index: {index}\n";

                if (NeedPathfindingData())
                {
                    if(_tacticsGrid.GridPathfinder.PathNodePool != null)
                    {
                        if (_tacticsGrid.GridPathfinder.PathNodePool.TryGetValue(index, out PathNode pathNode))
                        {
                            if (_showTerrainCost)
                                debugText += string.Format("terrain:{0:F1}\n", pathNode.terrainCost);

                            if (_showTraversalCost && pathNode.traversalCost != Mathf.Infinity)
                                debugText += string.Format("traversal:{0:F1}\n", pathNode.traversalCost);

                            if (_showHeuristicCost && pathNode.heuristicCost != Mathf.Infinity)
                                debugText += string.Format("heuristic:{0:F1}\n", pathNode.heuristicCost);

                            if (_showTotalCost && pathNode.totalCost != Mathf.Infinity)
                                debugText += string.Format("total:{0:F1}\n", pathNode.totalCost);
                        }
                    }
                }

                if (string.IsNullOrEmpty(debugText))
                {
                    DestroyTextGameObject(index);
                    return;
                }

                textObject.text = debugText;

                Vector3 tilePosition = tileData.tileMatrix.GetPosition();
                tilePosition.y += 0.1f;
                textObject.transform.position = tilePosition;
                textObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                textObject.transform.localScale = tileData.tileMatrix.lossyScale;
            }
            else
            {
                DestroyTextGameObject(index);
            }
        }

        public GameObject GetTextGameObject(GridIndex index)
        {
            if (_spawnedTexts.ContainsKey(index))
            {
                return _spawnedTexts[index];
            }
            else
            {
                GameObject newTextObject = Instantiate(_textObjectPrefab, Vector3.zero, Quaternion.identity).gameObject;
                _spawnedTexts.Add(index, newTextObject);
                return newTextObject;
            }
        }

        public void UpdateDebugText()
        {
            _showDebugText = ShowAnyDebug();
            if (!_showDebugText)
            {
                ClearAllTextGameObjects();
            }
            else
            {
                for (int i = 0; i < _tacticsGrid.GridTiles.Count; i++)
                {
                    UpdateTextOnTile(_tacticsGrid.GridTiles.ElementAt(i).Key);
                }
            }
        }

        public void DestroyTextGameObject(GridIndex index)
        {
            if (_spawnedTexts.TryGetValue(index, out GameObject textObject))
            {
                Destroy(textObject);
                _spawnedTexts.Remove(index);
            }
        }

        public void ClearAllTextGameObjects()
        {
            for (int i = 0; i < _spawnedTexts.Count; i++)
            {
                Destroy(_spawnedTexts.ElementAt(i).Value);
            }
            _spawnedTexts.Clear();
        }
    }
}
