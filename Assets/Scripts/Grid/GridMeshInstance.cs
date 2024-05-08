using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public struct InstanceData
    {
        public Matrix4x4 objectToWorld;
        public uint renderingLayerMask;
    }

    [ExecuteInEditMode]
    public class GridMeshInstance : MonoBehaviour
    {
        private Dictionary<Vector2Int, TileData> _instancedTiles = new Dictionary<Vector2Int, TileData>();
        private Dictionary<Vector2Int, TileData> _selectedTiles = new Dictionary<Vector2Int, TileData>();
        private TileData _hoveredTile;
        private RenderParams _renderParams;
        private RenderParams _selectedParams;
        private RenderParams _hoveredParams;
        private Mesh _instancedMesh;
        private Material _instancedMaterial;
        private Material _selectedMaterial;
        private Material _hoveredMaterial;
        private Color _materialColor;


        private List<InstanceData> _defaultRenders = new List<InstanceData>();
        private List<InstanceData> _selectedRenders = new List<InstanceData>();
        private int _currentDefaultCount;
        private int _currentSelectedCount;

        public void AddInstance(TileData tileData)
        {
            if (_instancedTiles.ContainsKey(tileData.index))
                _instancedTiles[tileData.index] = tileData;
            else
                _instancedTiles.TryAdd(tileData.index, tileData);
        }

        public void RemoveInstance(TileData tileData)
        {
            if (_instancedTiles.ContainsKey(tileData.index))
                _instancedTiles.Remove(tileData.index);
            if (_selectedTiles.ContainsKey(tileData.index))
                _selectedTiles.Remove(tileData.index);

            if (tileData.tileStates.Contains(TileState.Hovered))
            {
                _hoveredTile = default(TileData);
            }
        }

        public void AddState(Vector2Int index, TileState state)
        {
            if (state == TileState.Hovered)
            {
                if (_selectedTiles.ContainsKey(index))
                {
                    _hoveredTile = _selectedTiles[index];
                }
                else
                {
                    if (_instancedTiles.TryGetValue(index, out TileData tileData))
                    {
                        _hoveredTile = tileData;
                    }
                }
            }
            if (state == TileState.Selected)
            {
                _selectedTiles.TryAdd(index, _hoveredTile);
            }
        }

        public void RemoveState(Vector2Int index, TileState state)
        {
            if (state == TileState.Hovered)
            {
                _hoveredTile = default(TileData);
            }
            if (state == TileState.Selected)
            {
                _selectedTiles.Remove(index, out TileData tileData);
            }
        }

        public void UpdateGroundOffset(float offset)
        {
            for (int i = 0; i < _instancedTiles.Count; i++)
            {
                TileData tileData = _instancedTiles.ElementAt(i).Value;
                Vector3 newPos = tileData.tileMatrix.GetPosition();
                newPos.y += offset;
                tileData.tileMatrix = Matrix4x4.TRS(newPos, tileData.tileMatrix.rotation, tileData.tileMatrix.lossyScale);
                _instancedTiles[tileData.index] = tileData;
            }
        }

        private void Update()
        {
            if (_instancedTiles.Count > 0)
            {
                //Performance Optimization: Linq created too many allocations which resulted in frequent framedrops when tile count was high.
                //if (_currentDefaultCount != _instancedTiles.Count)
                //{
                //    _defaultRenders.Clear();
                //    for (int i = 0; i < _instancedTiles.Count; i++)
                //    {
                //        InstanceData newData;
                //        newData.objectToWorld = _instancedTiles.ElementAt(i).Value.tileMatrix;
                //        newData.renderingLayerMask = 1 << 7;
                //        _defaultRenders.Add(newData);
                //    }
                //    _currentDefaultCount = _instancedTiles.Count;
                //}
                Graphics.RenderMeshInstanced(_renderParams, _instancedMesh, 0, _instancedTiles.Values.Select(t => t.tileMatrix).ToList());
            }
            if (_selectedTiles.Count > 0)
            {
                //Performance Optimization: Linq created too many allocations which resulted in frequent framedrops when tile count was high.
                //if(_currentSelectedCount != _selectedTiles.Count)
                //{
                //    _selectedRenders.Clear();
                //    for (int i = 0; i < _selectedTiles.Count; i++)
                //    {
                //        InstanceData newData;
                //        newData.objectToWorld = _selectedTiles.ElementAt(i).Value.tileMatrix;
                //        newData.renderingLayerMask = 1 << 6;
                //        _selectedRenders.Add(newData);
                //    }
                //    _currentSelectedCount = _selectedTiles.Count;
                //}
                Graphics.RenderMeshInstanced(_selectedParams, _instancedMesh, 0, _selectedTiles.Values.Select(t => t.tileMatrix).ToList());
            }
            if (!_hoveredTile.Equals(default(TileData)))
            {
                Graphics.RenderMeshInstanced(_hoveredParams, _instancedMesh, 0, new[] { _hoveredTile.tileMatrix });
            }
        }

        public void ClearInstances()
        {
            _instancedTiles.Clear();
            _selectedTiles.Clear();
            _hoveredTile = default(TileData);
        }

        public void UpdateGridMeshInstances(Mesh mesh, Material material, Color color, List<TileData> gridTiles)
        {
            _instancedMesh = mesh;

            _instancedMaterial = new Material(material);
            _selectedMaterial = new Material(material);
            _hoveredMaterial = new Material(material);

            _materialColor = color;

            _instancedMaterial.color = _materialColor;
            _selectedMaterial.color = Color.green;
            _selectedMaterial.SetFloat("_IsFilled", 1f);
            _hoveredMaterial.color = Color.yellow;
            _hoveredMaterial.SetFloat("_IsFilled", 0.5f);

            _renderParams = new RenderParams(_instancedMaterial);
            _renderParams.rendererPriority = 0;
            _renderParams.worldBounds = this.GetComponent<TacticsGrid>().GetGridBounds();
            _selectedParams = new RenderParams(_selectedMaterial);
            _selectedParams.rendererPriority = 1;
            _hoveredParams = new RenderParams(_hoveredMaterial);
            _hoveredParams.rendererPriority = 2;

            _defaultRenders.Clear();
            _selectedRenders.Clear();

            gridTiles.ForEach(tile =>
            {
                _instancedTiles.Add(tile.index, tile);
            });

            _currentDefaultCount = 0;
            _currentSelectedCount = 0;
        }
    }
}