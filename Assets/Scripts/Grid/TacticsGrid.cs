using System;
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
        public event Action<GridIndex> OnTileDataUpdated;
        public event Action OnGridDestroyed;

        [SerializeField] private GridShape _gridShapeToggle = GridShape.Square;
        [SerializeField] private GridIndex _gridTileCount;
        [SerializeField] private Vector3 _gridTileSize;
        [SerializeField] private bool _useEnvironment = false;

        [SerializeField] private GridVisual _gridVisual;

        [SerializeField] private GridPathfinding _gridPathfinder;

        public Dictionary<GridIndex, TileData> GridTiles { get => _gridTiles; }
        public GridVisual GridVisual { get => _gridVisual; }
        public GridIndex GridTileCount { get => _gridTileCount; set => _gridTileCount = value; }
        public Vector3 TileSize { get => _gridTileSize; set => _gridTileSize = value; }
        public GridShape GridShape { get => _gridShape; set { _gridShape = value; } }
        public bool UseEnvironment { get => _useEnvironment; set => _useEnvironment = value; }

        public GridPathfinding GridPathfinder { get => _gridPathfinder; }

        private Vector3 _gridPosition = Vector3.zero;
        private GridShape _gridShape = GridShape.None;

        private Dictionary<GridIndex, TileData> _gridTiles = new Dictionary<GridIndex, TileData>();

        /* Bro, I don't know what's going on with this method, but it keeps disabling the script */
        //private void Awake()
        //{
        //    SpawnGrid(this.transform.position, _gridTileSize, _gridTileCount, _gridShapeToggle);
        //}

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

        public GridShapeData GetCurrentShapeData()
        {
            return DataManager.GetShapeData(_gridShape);
        }

        public bool IsIndexValid(GridIndex index)
        {
            return _gridTiles.ContainsKey(index);
        }

        public Vector3 GetCursorPositionOnGrid()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask groundLayer = LayerMask.GetMask("Ground");
            if(Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, groundLayer))
            {
                Vector3 hitPoint = hitInfo.point;
                return hitPoint;
            }
            else
            {
                Plane gridPlane = new Plane(Vector3.up, new Vector3(0, this.transform.position.y, 0));
                if (gridPlane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    return hitPoint;
                }
            }
            return new Vector3(-999, -999, -999);
        }

        public List<GridIndex> GetAllTilesWithState(TileState tileState)
        {
            List<GridIndex> tiles = new List<GridIndex>();
            for(int i = 0; i < _gridTiles.Count; i++)
            {
                HashSet<TileState> tileStates = _gridTiles.ElementAt(i).Value.tileStates;
                if (tileStates != null && tileStates.Contains(tileState))
                {
                    tiles.Add(_gridTiles.ElementAt(i).Key);
                }
            }
            return tiles;
        }

        public void ClearStateFromTiles(TileState stateToClear)
        {
            List<GridIndex> tilesWithState = GetAllTilesWithState(stateToClear);
            for(int i = 0; i < tilesWithState.Count; i++)
            {
                RemoveStateFromTile(tilesWithState[i], stateToClear);
            }
        }

        public Bounds GetGridBounds()
        {
            //TODO: This still doesn't work properly for hex/triangle grids
            Vector3 gridCenter = GetGridCenterPosition();
            //gridCenter.y += _gridTileSize.y / 2; //Do we want this here? Will this cause confusion later? *Prevents bounds being underneathe grid plane.
            float gridWidth = _gridTileCount.x * _gridTileSize.x;
            float gridHeight = _gridTileCount.z * _gridTileSize.y;

            return new Bounds(gridCenter, new Vector3(gridWidth, _gridTileSize.y, gridHeight));
        }

        public Vector3 GetGridCenterPosition()
        {
            Vector3 startPos = this.transform.position - _gridTileSize / 2;
            startPos.y = this.transform.position.y + 0.1f;
            Vector3 widthPos = new Vector3(startPos.x + _gridTileSize.x * _gridTileCount.x / 2, startPos.y, startPos.z);
            Vector3 widthHeightPos = new Vector3(widthPos.x, startPos.y, widthPos.z + _gridTileSize.z * _gridTileCount.z / 2);

            return widthHeightPos;
        }

        public GridIndex GetTileIndexUnderCursor()
        {
            return GetTileIndexFromWorldPosition(GetCursorPositionOnGrid());
        }

        public GridIndex GetTileIndexFromWorldPosition(Vector3 worldPosition)
        {
            GridIndex tileIndex = new GridIndex(-999, -999);
            Vector3 gridPosition = worldPosition - this.transform.position;

            switch (_gridShape)
            {
                case GridShape.Square:
                    tileIndex = CalculateIndexForSquare(gridPosition);
                    break;
                case GridShape.Hexagon:
                    tileIndex = CalculateIndexForHexagon(worldPosition);
                    break;
                case GridShape.Triangle:
                    tileIndex = CalculateIndexForTriangle(gridPosition);
                    break;
                default:
                    break;
            }

            return tileIndex;
        }

        private GridIndex CalculateIndexForSquare(Vector3 gridPosition)
        {
            Vector3 snappedPosition = GridStatics.SnapVectorToVector(gridPosition, _gridTileSize);
            Vector2 gridPositionSnapped = new Vector2(snappedPosition.x, snappedPosition.z);
            return GridIndex.RoundToInt(gridPositionSnapped / _gridTileSize);
        }

        private GridIndex CalculateIndexForHexagon(Vector3 worldPosition)
        {
            int roughX = Mathf.RoundToInt((worldPosition.x - this.transform.position.x) / _gridTileSize.x);
            int roughZ = Mathf.RoundToInt((worldPosition.z - this.transform.position.z) / _gridTileSize.z / 0.75f);
            GridIndex roughIndex = GridIndex.RoundToInt(new Vector2(roughX, roughZ));

            bool isOddRow = roughZ % 2 == 1;

            List<GridIndex> neighborList = new List<GridIndex>
            {
                roughIndex + new GridIndex(-1, 0),
                roughIndex + new GridIndex(1, 0),

                roughIndex + new GridIndex(isOddRow ? 1 : -1, 1),
                roughIndex + new GridIndex(0, 1),

                roughIndex + new GridIndex(isOddRow ? 1 : -1, -1),
                roughIndex + new GridIndex(0, -1)
            };

            GridIndex closestPoint = roughIndex;

            neighborList.ForEach(n =>
            {
                if (Vector3.Distance(worldPosition, GetTilePositionFromGridIndex(n)) < Vector3.Distance(worldPosition, GetTilePositionFromGridIndex(closestPoint)))
                {
                    closestPoint = n;
                }
            });

            return closestPoint;
        }

        private GridIndex CalculateIndexForTriangle(Vector3 gridPosition)
        {
            Vector3 snapToVector = new Vector3(_gridTileSize.x / 2f, _gridTileSize.y / 1f, _gridTileSize.z / 1f);
            Vector3 snappedPosition = GridStatics.SnapVectorToVector(gridPosition, snapToVector);

            Vector2 vectorTwoPosition = new Vector2(snappedPosition.x, snappedPosition.z);

            return GridIndex.RoundToInt((vectorTwoPosition / _gridTileSize) * new Vector2(2f, 1f));
        }

        public Vector3 GetTilePositionFromGridIndex(GridIndex gridIndex)
        {
            Vector2 offset = gridIndex * Vector2.one;

            switch (_gridShape)
            {
                case GridShape.Square:
                    break;
                case GridShape.Hexagon:
                    offset.x += Mathf.Abs(gridIndex.z) % 2 == 1 ? 0.5f : 0.0f;
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

        public Quaternion GetTileRotationFromGridIndex(GridIndex gridIndex)
        {
            Vector3 rotationVector = new Vector3(-90.0f, 0.0f, 90.0f); //Imported meshes are using Unreal Coordinate System. Adjusting for Unity.

            if (_gridShape == GridShape.Triangle)
            {
                if (gridIndex.x % 2 != 0)
                {
                    rotationVector.y = 180.0f;
                }
                if (gridIndex.z % 2 != 0)
                {
                    rotationVector.y += 180.0f;
                }
            }
            return Quaternion.Euler(rotationVector);
        }

        public void SpawnGrid(Vector3 gridPosition, Vector3 tileSize, GridIndex tileCount, GridShape gridShape)
        {
            _gridPosition = gridPosition;
            _gridShape = gridShape;
            DestroyGrid();

            if (_gridShape != GridShape.None)
            {
                List<TileData> tilesToRender = new List<TileData>();
                for (int z = 0; z < _gridTileCount.z; ++z)
                {
                    for (int x = 0; x < _gridTileCount.x; ++x)
                    {
                        TileData tileData = new TileData(); //Do we need TileData? With the fact we're using RenderMeshInstanced, there's never a need to find an instance. Just update the list of rendering stuff.
                        tileData.index = new GridIndex(x, z);

                        Vector3 instancePosition = GetTilePositionFromGridIndex(new GridIndex(x, z));
                        Quaternion instanceRotation = GetTileRotationFromGridIndex(new GridIndex(x, z));
                        Vector3 instanceScale = _gridTileSize;

                        if (!_useEnvironment)
                        {
                            tileData.tileType = TileType.Normal;
                            tileData.tileMatrix = Matrix4x4.TRS(instancePosition, instanceRotation, instanceScale);
                            AddGridTile(tileData);
                            tilesToRender.Add(tileData);
                        }
                        else
                        {
                            TileType tileType = TraceForGround(instancePosition, out Vector3 hitPosition);
                            if (GridStatics.IsTileTypeWalkable(tileType))
                            {
                                tileData.tileType = tileType;
                                tileData.tileMatrix = Matrix4x4.TRS(hitPosition, instanceRotation, instanceScale);
                                AddGridTile(tileData);
                                tilesToRender.Add(tileData);
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

        public TileType TraceForGround(Vector3 position, out Vector3 hitPosition)
        {
            TileType returnType = TileType.None;
            hitPosition = position;

            Vector3 origin = position + Vector3.up * 10.0f;
            LayerMask groundLayer = LayerMask.GetMask("Ground");
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
                hitPosition.y = sphereHits[0].point.y;
                //TODO: Add snap to grid?
            }

            return returnType;
        }

        public void AddGridTile(TileData tileData)
        {
            if (_gridTiles.ContainsKey(tileData.index))
                _gridTiles[tileData.index] = tileData;
            else
                _gridTiles.Add(tileData.index, tileData);

            _gridVisual.UpdateTileVisual(tileData);

            OnTileDataUpdated?.Invoke(tileData.index);
        }

        public void RemoveGridTile(GridIndex index)
        {
            if(_gridTiles.Remove(index, out TileData tileData))
            {
                tileData.tileType = TileType.None;
                _gridVisual.UpdateTileVisual(tileData);

                OnTileDataUpdated?.Invoke(index);
            }
        }

        public void AddStateToTile(GridIndex index, TileState tileState)
        {
            if (_gridTiles.ContainsKey(index))
            {
                TileData tileData = _gridTiles[index];

                if (tileData.tileStates == null)
                    tileData.tileStates = new HashSet<TileState>();

                if (tileData.tileStates.Add(tileState))
                {
                    _gridTiles[index] = tileData;
                    //_gridVisual.UpdateTileVisual(tileData);
                    _gridVisual.AddTileState(index, tileState);
                }

                OnTileDataUpdated?.Invoke(index);
            }
        }

        public void RemoveStateFromTile(GridIndex index, TileState tileState)
        {
            if (_gridTiles.ContainsKey(index))
            {
                TileData tileData = _gridTiles[index];

                if (tileData.tileStates == null)
                    return;

                if (tileData.tileStates.Contains(tileState))
                    tileData.tileStates.Remove(tileState);
                else
                    return;

                _gridTiles[index] = tileData;
                //_gridVisual.UpdateTileVisual(tileData);
                _gridVisual.RemoveTileState(index, tileState);

                OnTileDataUpdated?.Invoke(index);
            }
        }

        public void DestroyGrid()
        {
            _gridTiles.Clear();
            _gridVisual.ClearGridVisual();

            OnGridDestroyed?.Invoke();
        }
    }
}
