using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    [ExecuteInEditMode]
    public class TacticsGrid : MonoBehaviour
    {
        [SerializeField] private GridShape _gridShapeToggle = GridShape.Square;
        [SerializeField] private List<GridShapeData> _gridShapeData;
        [SerializeField] private int _gridWidth = 5;
        [SerializeField] private int _gridHeight = 5;
        [SerializeField] private Vector3 _gridTileSize;

        public int InstanceCount => _gridWidth * _gridHeight;

        public int GridWidth { get => _gridWidth; set => _gridWidth = value; }
        public int GridHeight { get => _gridHeight; set => _gridHeight = value; }
        public Vector3 TileSize { get => _gridTileSize; set => _gridTileSize = value; }
        public GridShape GridShape
        {
            get => _gridShape; set
            {
                _gridShape = value;
                if (_gridShape == GridShape.None) _isRendering = false; else _isRendering = true;
                SpawnGrid(this.transform.position, _gridTileSize, new Vector2Int(_gridWidth, _gridHeight), _gridShape);
            }
        }

        public bool ShowDebugLines { get => _showDebugLines; set => _showDebugLines = value; }
        public bool ShowDebugCenter { get => _showDebugCenter; set => _showDebugCenter = value; }
        public bool ShowDebugStart { get => _showDebugStart; set => _showDebugStart = value; }

        private Mesh _instancedMesh;
        private Material _instancedMaterial;
        private Vector3 _gridPosition = Vector3.zero;
        private Vector2Int _tileCount;
        private GridShape _gridShape;

        private bool _isRendering = true;
        private bool _showDebugLines = false;
        private bool _showDebugCenter = false;
        private bool _showDebugStart = false;

        private void Awake()
        {
            SpawnGrid(Vector3.zero, Vector3.one, new Vector2Int(_gridWidth, _gridHeight), _gridShapeToggle);
        }

        private void OnValidate()
        {
            if (_gridShapeToggle != _gridShape)
            {
                SpawnGrid(this.transform.position, _gridTileSize, new Vector2Int(_gridWidth, _gridHeight), _gridShapeToggle);
            }
        }

        private void Update()
        {
            if (!_isRendering)
                return;

            RenderParams renderParams = new RenderParams(_instancedMaterial);
            Matrix4x4[] instanceData = new Matrix4x4[_gridWidth * _gridHeight];
            int instanceIndex = 0;

            _gridPosition = GridUtilities.SnapVectorToVector(this.transform.position, _gridTileSize);
            _gridPosition.y = this.transform.position.y;
            this.transform.position = _gridPosition;

            for (int z = 0; z < _gridHeight; ++z)
            {
                for (int x = 0; x < _gridWidth; ++x)
                {
                    Vector3 instancePosition = GetTilePositionFromGridIndex(new Vector2Int(x, z));

                    Quaternion instanceRotation = GetTileRotationFromGridIndex(new Vector2Int(x, z));

                    Vector3 instanceScale = _gridTileSize;

                    instanceData[instanceIndex] = Matrix4x4.TRS(instancePosition, instanceRotation, instanceScale);
                    instanceIndex++;
                }
            }
            if (instanceIndex > 0)
                Graphics.RenderMeshInstanced(renderParams, _instancedMesh, 0, instanceData);
        }

        private void FixedUpdate()
        {
            //TODO: Debug Center and Lines don't work accurately for hexagon and Triangles. Fix later
            if (_showDebugLines)
            {
                Vector3 startPos = this.transform.position - _gridTileSize / 2;
                startPos.y = this.transform.position.y + 1.0f;
                Vector3 widthPos = new Vector3(startPos.x + _gridTileSize.x * _gridWidth, startPos.y, startPos.z);
                if (_gridShape == GridShape.Hexagon)
                    widthPos.x += _gridTileSize.x / 2;
                if (_gridShape == GridShape.Triangle)
                    widthPos.x /= 2;
                Vector3 widthHeightPos = new Vector3(widthPos.x, startPos.y, widthPos.z + _gridTileSize.z * _gridHeight);
                if (_gridShape == GridShape.Hexagon)
                    widthHeightPos.z *= 0.75f;
                Vector3 heightPos = new Vector3(startPos.x, startPos.y, widthHeightPos.z);
                Debug.DrawLine(startPos, widthPos, Color.yellow);
                Debug.DrawLine(widthPos, widthHeightPos, Color.yellow);
                Debug.DrawLine(startPos, heightPos, Color.yellow);
                Debug.DrawLine(heightPos, widthHeightPos, Color.yellow);
            }

            if (_showDebugCenter)
            {
                Vector3 startPos = this.transform.position - _gridTileSize / 2;
                startPos.y = this.transform.position.y + 1.0f;
                Vector3 widthPos = new Vector3(startPos.x + _gridTileSize.x * _gridWidth / 2, startPos.y, startPos.z);
                Vector3 widthHeightPos = new Vector3(widthPos.x, startPos.y, widthPos.z + _gridTileSize.z * _gridHeight / 2);
                DebugExtension.DebugWireSphere(widthHeightPos, Color.yellow, 0.25f);
            }

            if (_showDebugStart)
            {
                Vector3 startPos = this.transform.position;
                startPos.y += 1.0f;
                DebugExtension.DebugWireSphere(startPos, Color.yellow, 0.25f);
            }
        }

        private Vector3 GetTilePositionFromGridIndex(Vector2Int gridIndex)
        {
            Vector2 offset = gridIndex * Vector2.one;

            switch (_gridShape)
            {
                case GridShape.Square:
                    break;
                case GridShape.Hexagon:
                    offset.x += gridIndex.y % 2 == 1 ? 0.5f : 0.0f;
                    offset.y *= 0.75f;
                    break;
                case GridShape.Triangle:
                    offset *= new Vector2(0.5f, 1.0f);
                    break;
                default:
                    break;
            }

            Vector3 tilePosition = new Vector3(
                this.transform.position.x + offset.x * _gridTileSize.x,
                this.transform.position.y,
                this.transform.position.z + offset.y * _gridTileSize.z
                );

            return tilePosition;
        }

        private Quaternion GetTileRotationFromGridIndex(Vector2Int gridIndex)
        {
            Vector3 rotationVector = new Vector3(-90.0f, 0.0f, 90.0f); //Imported meshes are using Unreal Coordinate System. Adjusting for Unity.

            if (_gridShape == GridShape.Triangle)
            {
                if (gridIndex.x % 2 != 0)
                {
                    rotationVector.y = 180.0f;
                }
                if (gridIndex.y % 2 != 0)
                {
                    rotationVector.y += 180.0f;
                }
            }
            return Quaternion.Euler(rotationVector);
        }

        public void SpawnGrid(Vector3 gridPosition, Vector3 tileSize, Vector2Int tileCount, GridShape gridShape)
        {
            _gridPosition = gridPosition;
            _tileCount = tileCount;
            _gridShape = gridShape;

            GridShapeData activeData = _gridShapeData.Find(data => data.gridShape == gridShape);
            if (activeData != null)
            {
                _instancedMesh = activeData.flatMesh;
                _instancedMaterial = activeData.flatFilledMaterial;
            }
        }

        public GridShapeData GetCurrentShapeData()
        {
            return _gridShapeData[(int)_gridShapeToggle];
        }
    }
}
