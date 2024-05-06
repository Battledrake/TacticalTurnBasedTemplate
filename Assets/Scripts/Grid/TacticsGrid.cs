using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public enum TileType
    {
        None,
        Normal,
        Obstacle
    }

    [ExecuteInEditMode]
    public class TacticsGrid : MonoBehaviour
    {
        [SerializeField] private GridShape _gridShapeToggle = GridShape.Square;
        [SerializeField] private Vector2Int _gridTileCount;
        [SerializeField] private Vector3 _gridTileSize;
        [SerializeField] private bool _useEnvironment = false;

        [SerializeField] private GridVisual _gridVisual;

        public GridVisual GridVisual { get => _gridVisual; }
        public Vector2Int GridTileCount { get => _gridTileCount; set => _gridTileCount = value; }
        public Vector3 TileSize { get => _gridTileSize; set => _gridTileSize = value; }
        public GridShape GridShape { get => _gridShape; set { _gridShape = value; } }
        public bool UseEnvironment { get => _useEnvironment; set => _useEnvironment = value; }

        public bool ShowDebugLines { get => _showDebugLines; set => _showDebugLines = value; }
        public bool ShowDebugCenter { get => _showDebugCenter; set => _showDebugCenter = value; }
        public bool ShowDebugStart { get => _showDebugStart; set => _showDebugStart = value; }

        private RenderParams _renderParams;
        private List<Matrix4x4> _instanceData = new List<Matrix4x4>();

        private Vector3 _gridPosition = Vector3.zero;
        private GridShape _gridShape = GridShape.None;

        private bool _showDebugLines = false;
        private bool _showDebugCenter = false;
        private bool _showDebugStart = false;

        private Dictionary<Vector2Int, TileData> _gridTiles = new Dictionary<Vector2Int, TileData>();

        private void AddGridTile(TileData tileData)
        {
            if (_gridTiles.ContainsKey(tileData.index))
                _gridTiles[tileData.index] = tileData;
            else
                _gridTiles.Add(tileData.index, tileData);

            _gridVisual.UpdateTileVisual(tileData);
        }

        private void Awake()
        {
            SpawnGrid(this.transform.position, _gridTileSize, _gridTileCount, _gridShapeToggle);
        }

        private void OnValidate()
        {
            SpawnGrid(this.transform.position, _gridTileSize, _gridTileCount, _gridShapeToggle);
        }

        private void Update()
        {
            if (_gridShape == GridShape.None)
                return;

            if (this.transform.position != _gridPosition)
            {
                _gridPosition = GridStatics.SnapVectorToVector(this.transform.position, _gridTileSize);
                _gridPosition.y = this.transform.position.y;
                this.transform.position = _gridPosition;
                RespawnGrid();
            }
        }

        private void FixedUpdate()
        {
            //TODO: Debug Center and Lines don't work accurately for hexagon and Triangles. Fix later
            if (_showDebugLines)
            {
                Vector3 startPos = this.transform.position - _gridTileSize / 2;
                startPos.y = this.transform.position.y + 0.1f;
                Vector3 widthPos = new Vector3(startPos.x + _gridTileSize.x * _gridTileCount.x, startPos.y, startPos.z);
                if (_gridShape == GridShape.Hexagon)
                    widthPos.x += _gridTileSize.x / 2;
                if (_gridShape == GridShape.Triangle)
                    widthPos.x /= 2;
                Vector3 widthHeightPos = new Vector3(widthPos.x, startPos.y, widthPos.z + _gridTileSize.z * _gridTileCount.y);
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
                startPos.y = this.transform.position.y + 0.1f;
                Vector3 widthPos = new Vector3(startPos.x + _gridTileSize.x * _gridTileCount.x / 2, startPos.y, startPos.z);
                Vector3 widthHeightPos = new Vector3(widthPos.x, startPos.y, widthPos.z + _gridTileSize.z * _gridTileCount.y / 2);
                DebugExtension.DebugWireSphere(widthHeightPos, Color.yellow, 0.25f);
            }

            if (_showDebugStart)
            {
                Vector3 startPos = this.transform.position;
                startPos.y += 0.1f;
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
            _gridShape = gridShape;

            _instanceData.Clear();

            if (_gridShape != GridShape.None)
            {
                List<Matrix4x4> tilesToRender = new List<Matrix4x4>();
                for (int z = 0; z < _gridTileCount.y; ++z)
                {
                    for (int x = 0; x < _gridTileCount.x; ++x)
                    {
                        TileData tileData = new TileData(); //Do we need TileData? With the fact we're using RenderMeshInstanced, there's never a need to find an instance. Just update the list of rendering stuff.
                        tileData.index = new Vector2Int(x, z);

                        Vector3 instancePosition = GetTilePositionFromGridIndex(new Vector2Int(x, z));
                        Quaternion instanceRotation = GetTileRotationFromGridIndex(new Vector2Int(x, z));
                        Vector3 instanceScale = _gridTileSize;

                        if (!_useEnvironment)
                        {
                            tileData.tileType = TileType.Normal;
                            tileData.tileMatrix = Matrix4x4.TRS(instancePosition, instanceRotation, instanceScale);
                            AddGridTile(tileData);
                            tilesToRender.Add(tileData.tileMatrix);
                        }
                        else
                        {
                            TileType tileType = TraceForGround(instancePosition, out Vector3 hitLocation);
                            if (GridStatics.IsTileTypeWalkable(tileType))
                            {
                                tileData.tileType = tileType;
                                tileData.tileMatrix = Matrix4x4.TRS(hitLocation, instanceRotation, instanceScale);
                                AddGridTile(tileData);
                                tilesToRender.Add(tileData.tileMatrix);
                            }
                        }
                    }
                }
                if (tilesToRender.Count > 0)
                    _gridVisual.UpdateGridVisual(GetCurrentShapeData(), tilesToRender);
                else
                    _gridVisual.ClearGridVisual();
            }
        }

        public void RespawnGrid()
        {
            SpawnGrid(this.transform.position, _gridTileSize, _gridTileCount, _gridShape);
        }

        public void DestroyGrid()
        {
            _gridTiles.Clear();
            _gridVisual.ClearGridVisual();
        }

        public GridShapeData GetCurrentShapeData()
        {
            return DataManager.GetShapeData(_gridShape);
        }

        public TileType TraceForGround(Vector3 position, out Vector3 hitLocation)
        {
            TileType returnType = TileType.None;
            hitLocation = position;

            Vector3 origin = position + Vector3.up * 10.0f;
            LayerMask groundLayer = (1 << LayerMask.NameToLayer("Ground"));
            float radius = _gridTileSize.x / 3; //TODO: divide by 5 if triangle?
            RaycastHit[] sphereHits = Physics.SphereCastAll(origin, radius, Vector3.down, 20.0f, groundLayer);
            if (sphereHits.Length > 0)
            {
                returnType = TileType.Normal;
                for (int i = 0; i < sphereHits.Length; i++)
                {
                    GridModifier gridModifier = sphereHits[i].collider.GetComponent<GridModifier>();
                    if (gridModifier)
                    {
                        returnType = gridModifier.TileType;
                    }
                }
                hitLocation.y = sphereHits[0].point.y;
                //TODO: Add snap to grid?
            }

            return returnType;
        }
    }
}
