using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
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

        private Dictionary<GridIndex, TextMeshPro> _spawnedTexts = new Dictionary<GridIndex, TextMeshPro>();

        private bool _showTileIndexes = false;
        private bool _showTerrainCost = false;
        private bool _showTraversalCost = false;
        private bool _showHeuristicCost = false;
        private bool _showTotalCost = false;

        private void OnEnable()
        {
            _tacticsGrid.OnTileDataUpdated += (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed += ClearAllTextGameObjects;
            _tacticsGrid.GridPathfinder.OnPathfindingCompleted += UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataCleared += UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataUpdated += UpdateTextOnTile;
        }

        private void OnDisable()
        {
            _tacticsGrid.OnTileDataUpdated -= (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed -= ClearAllTextGameObjects;
            _tacticsGrid.GridPathfinder.OnPathfindingCompleted -= UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataCleared -= UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataUpdated -= UpdateTextOnTile;
        }

        private bool ShowAnyDebug()
        {
            return _showTileIndexes || _showTerrainCost || NeedPathfindingData();
        }

        private bool NeedPathfindingData()
        {
            return _showTraversalCost || _showHeuristicCost || _showTotalCost;
        }

        public void UpdateTextOnAllTiles()
        {
            if (!ShowAnyDebug())
                return;

            for(int i = 0; i < _tacticsGrid.GridTiles.Count; i++)
            {
                UpdateTextOnTile(_tacticsGrid.GridTiles.ElementAt(i).Key);
            }
        }

        public void UpdateTextOnTile(GridIndex index)
        {
            if (ShowAnyDebug() && _tacticsGrid.GridTiles.TryGetValue(index, out TileData tileData) && GridStatics.IsTileTypeWalkable(tileData.tileType))
            {
                string debugText = "";

                TextMeshPro textMeshComp = GetTextGameObject(index);

                if (_tacticsGrid.GridShape == GridShape.Triangle)
                    textMeshComp.fontSize = 0.75f;
                if (_tacticsGrid.GridShape == GridShape.Hexagon)
                    textMeshComp.fontSize = 1f;

                if (_showTileIndexes)
                    debugText += $"index: {index}\n";

                if (_showTerrainCost)
                    debugText += string.Format("terrain:{0:F1}\n", (int)tileData.tileType);

                if (NeedPathfindingData())
                {
                    if(_tacticsGrid.GridPathfinder.PathNodePool != null)
                    {
                        if (_tacticsGrid.GridPathfinder.PathNodePool.TryGetValue(index, out PathNode pathNode))
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

                if (string.IsNullOrEmpty(debugText))
                {
                    DestroyTextGameObject(index);
                    return;
                }

                textMeshComp.text = debugText;

                Vector3 tilePosition = tileData.tileMatrix.GetPosition();
                tilePosition.y += 0.1f;
                textMeshComp.transform.position = tilePosition;
                textMeshComp.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                textMeshComp.transform.localScale = tileData.tileMatrix.lossyScale;
            }
            else
            {
                DestroyTextGameObject(index);
            }
        }

        public TextMeshPro GetTextGameObject(GridIndex index)
        {
            if (_spawnedTexts.ContainsKey(index))
            {
                return _spawnedTexts[index];
            }
            else
            {
                TextMeshPro newTextObject = Instantiate(_textObjectPrefab, Vector3.zero, Quaternion.identity);
                _spawnedTexts.Add(index, newTextObject);
                return newTextObject;
            }
        }

        public void UpdateDebugText()
        {
            if (!ShowAnyDebug())
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
            if (_spawnedTexts.TryGetValue(index, out TextMeshPro textObject))
            {
                Destroy(textObject.gameObject);
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
