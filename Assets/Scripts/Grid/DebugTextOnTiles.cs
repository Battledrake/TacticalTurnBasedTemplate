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

        private Dictionary<Vector2Int, GameObject> _spawnedTexts = new Dictionary<Vector2Int, GameObject>();

        private bool _showTileText;

        private void OnEnable()
        {
            _tacticsGrid.TileDataUpdated += (i => UpdateTextOnTile(i));
            _tacticsGrid.GridDestroyed += ClearAllTextGameObjects;
        }

        private void OnDisable()
        {
            _tacticsGrid.TileDataUpdated -= (i => UpdateTextOnTile(i));
            _tacticsGrid.GridDestroyed -= ClearAllTextGameObjects;
        }

        public bool UpdateTextOnTile(Vector2Int index)
        {
            if (_showTileText && _tacticsGrid.GridTiles.TryGetValue(index, out TileData tileData) && GridStatics.IsTileTypeWalkable(tileData.tileType))
            {

                TextMeshPro textObject = GetTextGameObject(index).GetComponent<TextMeshPro>();

                textObject.text = string.Format("{0},{1}", index.x, index.y);

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
            return false;
        }

        public GameObject GetTextGameObject(Vector2Int index)
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

        public void ShowTileText(bool showText)
        {
            _showTileText = showText;
            if (!_showTileText)
            {
                ClearAllTextGameObjects();
            }
            else
            {
                for(int i = 0; i < _tacticsGrid.GridTiles.Count; i++)
                {
                    UpdateTextOnTile(_tacticsGrid.GridTiles.ElementAt(i).Key);
                }
            }
        }

        public void DestroyTextGameObject(Vector2Int index)
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
