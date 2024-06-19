using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum TileType
    {
        None,
        Normal,
        DoubleCost,
        TripleCost,
        FlyingOnly,
        Obstacle
    }

    public enum CoverType
    {
        None,
        HalfCover,
        FullCover
    }

    public struct CoverData
    {
        public CoverType coverType;
        public GridIndex direction;
    }

    public struct Cover
    {
        public bool hasCover;
        public List<CoverData> data;
    }

    public struct ClimbData
    {
        public bool hasClimbLink;
        public List<GridIndex> climbLinks;
    }

    public struct TileData
    {
        public GridIndex index;
        public TileType tileType;
        public Matrix4x4 tileMatrix;
        public HashSet<TileState> tileStates;
        public Unit unitOnTile;
        public ClimbData climbData;
        public Cover cover;
    }

    public class TacticsGrid : MonoBehaviour
    {
        public event Action<GridIndex> OnTileDataUpdated;
        public event Action<GridIndex> OnTileHeightChanged;
        public event Action OnGridDestroyed;
        public event Action OnGridGenerated;

        [Header("Grid Configuration")]
        [SerializeField] private GridIndex _gridTileCount;
        [SerializeField] private Vector3 _gridTileSize;
        [SerializeField] private bool _useEnvironment = false;

        [Header("Dependencies")]
        [SerializeField] private GridVisual _gridVisual;
        [SerializeField] private GridPathfinding _pathfinder;

        public Dictionary<GridIndex, TileData> GridTiles => _gridTiles;
        public GridVisual GridVisual => _gridVisual;
        public GridIndex GridTileCount { get => _gridTileCount; set => _gridTileCount = value; }
        public Vector3 TileSize { get => _gridTileSize; set => _gridTileSize = value; }
        public GridShape GridShape { get => _gridShape; set => _gridShape = value; }
        public bool UseEnvironment { get => _useEnvironment; set => _useEnvironment = value; }
        public GridPathfinding Pathfinder => _pathfinder;
        public Dictionary<GridIndex, Cover> Covers => _covers;

        private Vector3 _gridPosition = Vector3.zero;
        private GridShape _gridShape = GridShape.Square;

        private Dictionary<GridIndex, TileData> _gridTiles = new();
        private Dictionary<TileState, HashSet<GridIndex>> _tileStateIndexes = new();
        private Dictionary<GridIndex, Cover> _covers = new();

        private void Start()
        {
            RespawnGrid();
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

        public bool GetTileDataFromIndex(GridIndex index, out TileData tileData)
        {
            if (IsIndexValid(index))
            {
                tileData = _gridTiles[index];
                return true;
            }
            else
            {
                tileData = default(TileData);
                return false;
            }
        }

        public Vector3 GetTilePositionFromIndex(GridIndex index)
        {
            if (_gridTiles.TryGetValue(index, out TileData tileData))
            {
                return tileData.tileMatrix.GetPosition();
            }
            return Vector3.zero;
        }

        public GridShapeData GetCurrentShapeData()
        {
            return DataManager.GetGridShapeData(_gridShape);
        }

        public bool IsIndexValid(GridIndex index)
        {
            return _gridTiles.ContainsKey(index);
        }

        public Vector3 GetCursorPositionOnGrid()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return new Vector3(-999, -999, -999);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask groundLayer = LayerMask.GetMask("Ground");
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, groundLayer))
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
            if (_tileStateIndexes.TryGetValue(tileState, out HashSet<GridIndex> stateIndexes))
                return stateIndexes.ToList();
            return new List<GridIndex>();
        }

        public void ClearAllTilesWithState(TileState stateToClear)
        {
            if (_tileStateIndexes.TryGetValue(stateToClear, out HashSet<GridIndex> indexesWithState))
            {
                foreach (GridIndex index in indexesWithState)
                {
                    RemoveStateFromTile(index, stateToClear);
                }
                indexesWithState.Clear();
            }
        }

        public void ClearStateFromTiles(List<GridIndex> tiles, TileState stateToClear)
        {


            _tileStateIndexes.TryGetValue(stateToClear, out HashSet<GridIndex> stateIndexes);

            for (int i = 0; i < tiles.Count; i++)
            {
                if (stateIndexes != null)
                    stateIndexes.Remove(tiles[i]);

                RemoveStateFromTile(tiles[i], stateToClear);
            }
        }

        public Bounds GetGridBounds()
        {
            //TODO: This still doesn't work properly for hex/triangle grids
            Vector3 gridCenter = GetGridCenterPosition();
            //gridCenter.y += _gridTileSize.y / 2; //Do we want this here? Will this cause confusion later? *Prevents bounds being underneathe grid plane.
            float gridWidth = _gridTileCount.x * _gridTileSize.x;
            float gridHeight = _gridTileCount.z * _gridTileSize.z;

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

            bool isOddRow = Mathf.Abs(roughZ) % 2 == 1;

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
                if (Vector3.Distance(worldPosition, GetWorldPositionFromGridIndex(n)) < Vector3.Distance(worldPosition, GetWorldPositionFromGridIndex(closestPoint)))
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

        /// <summary>
        /// Gets the Vector3 position from a gridindex with y being the grid's base position. Mostly used for Grid Generation. Use GetTilePositionFromGridIndex or GetTileDataFromGridIndex instead if y position is needed.
        /// </summary>
        /// <param name="gridIndex"></param>
        /// <returns></returns>
        public Vector3 GetWorldPositionFromGridIndex(GridIndex gridIndex)
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
            //if (_gridShape == GridShape.Hexagon)
            //    rotationVector = Vector3.zero;

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
                        TileData tileData = new();
                        tileData.index = new GridIndex(x, z);

                        Vector3 instancePosition = GetWorldPositionFromGridIndex(new GridIndex(x, z));
                        Quaternion instanceRotation = GetTileRotationFromGridIndex(new GridIndex(x, z));
                        Vector3 instanceScale = _gridTileSize;

                        if (!_useEnvironment)
                        {
                            tileData.tileType = TileType.Normal;
                            tileData.tileMatrix = Matrix4x4.TRS(instancePosition, instanceRotation, instanceScale);
                            AddGridTileNoNotify(tileData);
                            tilesToRender.Add(tileData);
                        }
                        else
                        {
                            TileType tileType = TraceForGroundAndObstacles(instancePosition, out Vector3 hitPosition, out Vector3 hitNormal);
                            if (tileType != TileType.None)
                            {
                                if (GridStatics.IsTileTypeTraversable(tileType))
                                {
                                    List<GridIndex> climbLinks = TraceForClimbLinks(tileData.index, hitPosition);
                                    if (climbLinks.Count > 0)
                                    {
                                        tileData.climbData.hasClimbLink = true;
                                        tileData.climbData.climbLinks = climbLinks;
                                    }

                                    if (_gridShape == GridShape.Square)
                                    {
                                        Cover cover = TraceForCover(hitPosition);
                                        if (cover.hasCover)
                                        {
                                            tileData.cover = cover;
                                            _covers.TryAdd(tileData.index, cover);
                                        }
                                    }
                                }

                                tileData.tileType = tileType;
                                instanceRotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * instanceRotation;
                                tileData.tileMatrix = Matrix4x4.TRS(hitPosition, instanceRotation, instanceScale);
                                AddGridTileNoNotify(tileData);
                                tilesToRender.Add(tileData);
                            }
                        }
                    }
                }
                if (tilesToRender.Count > 0)
                {
                    _gridVisual.UpdateGridVisual(GetCurrentShapeData(), tilesToRender);
                }
                else
                {
                    _gridVisual.ClearGridVisual();
                }


                OnGridGenerated?.Invoke();
            }
        }

        private Cover TraceForCover(Vector3 originPosition)
        {
            GridIndex[] squareDir = new[] { new GridIndex(0, 1), new GridIndex(1, 0), new GridIndex(-1, 0), new GridIndex(0, -1) };
            Vector3 up = Vector3.up * 0.5f;
            LayerMask checkLayer = LayerMask.GetMask("Ground", "Obstacle");
            Cover cover = new();
            cover.hasCover = false;
            cover.data = new();
            for (int i = 0; i < squareDir.Length; i++)
            {
                Vector3 direction = new Vector3(squareDir[i].x, 0, squareDir[i].z);
                float distance = _gridTileSize.x;
                if(Physics.Raycast(originPosition + up, direction, distance, checkLayer))
                {
                    if (!cover.hasCover)
                        cover.hasCover = true;

                    CoverData coverData = new();
                    coverData.direction = squareDir[i];
                    coverData.coverType = CoverType.HalfCover;

                    if(Physics.Raycast(originPosition + Vector3.up * 1.75f, direction, distance, checkLayer))
                    {
                        coverData.coverType = CoverType.FullCover;
                    }
                    cover.data.Add(coverData);
                }
            }
            return cover;
        }

        private List<GridIndex> TraceForClimbLinks(GridIndex origin, Vector3 originPosition)
        {
            int neighborCount = 0;
            switch (_gridShape)
            {
                case GridShape.Square:
                    neighborCount = 4;
                    break;
                case GridShape.Hexagon:
                    neighborCount = 6;
                    break;
                case GridShape.Triangle:
                    neighborCount = 3;
                    break;
            }

            List<GridIndex> climbLinks = new List<GridIndex>();
            LayerMask climbLayer = LayerMask.GetMask("Climb");

            for (int i = 0; i < neighborCount; i++)
            {
                GridIndex neighborIndex = GridStatics.GetNeighborAtIndexFromShape(origin, i, _gridShape);
                Vector3 neighborPosition = GetWorldPositionFromGridIndex(neighborIndex);
                Vector3 direction = neighborPosition - originPosition;
                direction.y = 0f;
                direction.Normalize();

                if (Physics.Raycast(originPosition, direction, _gridTileSize.x, climbLayer))
                {
                    climbLinks.Add(neighborIndex);
                }
            }
            return climbLinks;
        }

        public bool IsTileWalkable(GridIndex index)
        {
            if (_gridTiles.ContainsKey(index))
                return (GridStatics.IsTileTypeTraversable(_gridTiles[index].tileType));
            else
                return false;
        }

        public void RespawnGrid()
        {
            SpawnGrid(this.transform.position, _gridTileSize, _gridTileCount, _gridShape);
        }

        public TileType TraceForGroundAndObstacles(Vector3 position, out Vector3 hitPosition, out Vector3 hitNormal)
        {
            TileType returnType = TileType.None;
            hitPosition = position;
            hitNormal = Vector3.up;

            Vector3 origin = position + Vector3.up * 10.0f;
            LayerMask groundLayer = LayerMask.GetMask("Ground", "Obstacle");
            //float radius = _gridTileSize.x / 3; //TODO: divide by 5 if triangle?
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitInfo, 50.0f, groundLayer))
            {
                returnType = TileType.Normal;


                GridModifier gridModifier = hitInfo.collider.GetComponent<GridModifier>();
                if (gridModifier)
                {
                    returnType = gridModifier.TileType;
                }
                hitPosition.y = hitInfo.point.y;
                hitNormal = hitInfo.normal;
            }

            return returnType;
        }

        public bool AddUnitToTile(GridIndex index, Unit unit, bool shouldPosition = true, bool forceEntry = false)
        {
            if (!IsIndexValid(index))
                return false;

            _gridTiles.TryGetValue(index, out TileData tileData);
            if (tileData.unitOnTile)
            {
                Debug.LogWarning($"Unit already on tile:{index}");
                if (!forceEntry)
                    return false;
            }
            tileData.unitOnTile = unit;
            unit.SetGridIndex(index);

            if (shouldPosition)
                unit.transform.position = tileData.tileMatrix.GetPosition();

            _gridTiles[index] = tileData;

            unit.SetUnitsGrid(this);

            OnTileDataUpdated?.Invoke(index);
            return true;
        }

        public bool RemoveUnitFromTile(GridIndex index)
        {
            _gridTiles.TryGetValue(index, out TileData tileData);
            if (tileData.unitOnTile)
            {
                tileData.unitOnTile.SetGridIndex(GridIndex.Invalid());

                tileData.unitOnTile = null;
                _gridTiles[index] = tileData;

                OnTileDataUpdated?.Invoke(index);
                return true;
            }
            return false;
        }

        public void AddGridTileNoNotify(TileData tileData)
        {
            if (_gridTiles.ContainsKey(tileData.index))
                _gridTiles[tileData.index] = tileData;
            else
                _gridTiles.Add(tileData.index, tileData);

            _gridVisual.UpdateTileVisual(tileData);
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

        public void ChangeTileHeight(TileData tileData)
        {
            if (_gridTiles.ContainsKey(tileData.index))
                _gridTiles[tileData.index] = tileData;
            else
                _gridTiles.Add(tileData.index, tileData);

            _gridVisual.UpdateTileVisual(tileData);

            OnTileHeightChanged?.Invoke(tileData.index);
        }

        public void RemoveGridTile(GridIndex index)
        {
            if (_gridTiles.Remove(index, out TileData tileData))
            {
                tileData.tileType = TileType.None;
                _gridVisual.UpdateTileVisual(tileData);

                if (tileData.tileStates != null)
                {
                    foreach (TileState tileState in tileData.tileStates)
                    {

                        _tileStateIndexes.TryGetValue(tileState, out HashSet<GridIndex> stateIndexes);
                        stateIndexes.Remove(index);
                    }
                }

                OnTileDataUpdated?.Invoke(index);
            }
        }

        public void AddStateToTile(GridIndex index, TileState tileState)
        {
            if (_gridTiles.TryGetValue(index, out TileData tileData))
            {
                if (tileData.tileStates == null)
                    tileData.tileStates = new HashSet<TileState>();

                if (tileData.tileStates.Add(tileState))
                {
                    _gridTiles[index] = tileData;
                    _gridVisual.AddTileState(index, tileState);

                    if (!_tileStateIndexes.TryGetValue(tileState, out HashSet<GridIndex> tileStateIndexes))
                    {
                        tileStateIndexes = new HashSet<GridIndex>();
                        _tileStateIndexes[tileState] = tileStateIndexes;
                    }

                    tileStateIndexes.Add(index);
                }

                OnTileDataUpdated?.Invoke(index);
            }
        }

        public void RemoveStateFromTile(GridIndex index, TileState tileState)
        {
            if (_gridTiles.TryGetValue(index, out TileData tileData))
            {
                if (tileData.tileStates == null)
                    return;

                if (tileData.tileStates.Contains(tileState))
                    tileData.tileStates.Remove(tileState);
                else
                    return;

                _gridTiles[index] = tileData;
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
