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
        public bool ShowUnitOnTile { get => _showUnitOnTile; set => _showUnitOnTile = value; }

        private Dictionary<GridIndex, GameObject> _spawnedTexts = new Dictionary<GridIndex, GameObject>();

        private bool _showTileIndexes = false;
        private bool _showTerrainCost = false;
        private bool _showTraversalCost = false;
        private bool _showHeuristicCost = false;
        private bool _showTotalCost = false;
        private bool _showUnitOnTile = false;

        private void OnEnable()
        {
            _tacticsGrid.OnGridGenerated += UpdateTextOnAllTiles;
            _tacticsGrid.OnTileDataUpdated += (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed += ClearAllTextGameObjects;
            _tacticsGrid.GridPathfinder.OnPathfindingCompleted += UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataCleared += UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataUpdated += UpdateTextOnAllTiles;
        }

        private void OnDisable()
        {
            _tacticsGrid.OnGridGenerated -= UpdateTextOnAllTiles;
            _tacticsGrid.OnTileDataUpdated -= (i => UpdateTextOnTile(i));
            _tacticsGrid.OnGridDestroyed -= ClearAllTextGameObjects;
            _tacticsGrid.GridPathfinder.OnPathfindingCompleted -= UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataCleared -= UpdateTextOnAllTiles;
            _tacticsGrid.GridPathfinder.OnPathfindingDataUpdated -= UpdateTextOnAllTiles;
        }

        private bool ShowAnyDebug()
        {
            return _showTileIndexes || _showTerrainCost || _showUnitOnTile || HasPathfindingData();
        }

        private bool HasPathfindingData()
        {
            return _showTraversalCost || _showHeuristicCost || _showTotalCost;
        }

        public void UpdateTextOnAllTiles()
        {
            if (!ShowAnyDebug())
                return;

            for (int i = 0; i < _tacticsGrid.GridTiles.Count; i++)
            {
                UpdateTextOnTile(_tacticsGrid.GridTiles.ElementAt(i).Key);
            }
        }

        public void UpdateTextOnTile(GridIndex index)
        {
            if (ShowAnyDebug() && _tacticsGrid.GridTiles.TryGetValue(index, out TileData tileData) && GridStatics.IsTileTypeWalkable(tileData.tileType))
            {
                string debugText = "";

                TextMeshPro textMeshComp = GetTextGameObject(index).GetComponent<TextMeshPro>();

                if (_tacticsGrid.GridShape == GridShape.Triangle)
                    textMeshComp.fontSize = 0.75f;
                if (_tacticsGrid.GridShape == GridShape.Hexagon)
                    textMeshComp.fontSize = 1f;

                if (_showTileIndexes)
                    debugText += $"index: {index}\n";

                if (_showTerrainCost)
                    debugText += string.Format("terrain:{0:F1}\n", PathfindingStatics.GetTerrainCostFromTileType(tileData.tileType));

                if (HasPathfindingData())
                {
                    if (_tacticsGrid.GridPathfinder.PathNodePool != null)
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

                if (_showUnitOnTile)
                {
                    string unitText = tileData.unitOnTile ? tileData.unitOnTile.name : "none";
                    debugText += string.Format("unit:{0}\n", unitText);
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
            if (_spawnedTexts.TryGetValue(index, out GameObject textObject))
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
