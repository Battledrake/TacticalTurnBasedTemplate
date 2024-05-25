using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class GridMeshInstancer : MonoBehaviour
    {
        [SerializeField] private Color _instanceColor = Color.black;
        [SerializeField] private Color _selectedColor = Color.green;
        [SerializeField] private Color _neighborColor = Color.magenta;
        [SerializeField] private Color _moveRangeColor = Color.green;
        [SerializeField] private Color _pathColor = Color.green;
        [SerializeField] private Color _abilityRangeColor = Color.yellow;
        [SerializeField] private Color _abilityAreaOfEffectColor = Color.yellow;
        [SerializeField] private Color _hoveredColor = Color.yellow;

        public bool ShowBaseGrid { get => _renderBaseGrid; set => _renderBaseGrid = value; }

        private Dictionary<GridIndex, TileData> _instancedTiles = new Dictionary<GridIndex, TileData>();
        private List<Matrix4x4> _selectedTiles = new List<Matrix4x4>();
        private List<Matrix4x4> _neighborTiles = new List<Matrix4x4>();
        private List<Matrix4x4> _moveRangeTiles = new List<Matrix4x4>();
        private List<Matrix4x4> _pathTiles = new List<Matrix4x4>();
        private List<Matrix4x4> _abilityRangeTiles = new List<Matrix4x4>();
        private List<Matrix4x4> _abilityAreaOfEffectTiles = new List<Matrix4x4>();
        private TileData _hoveredTile;

        private RenderParams _renderParams;
        private RenderParams _selectedParams;
        private RenderParams _neighborParams;
        private RenderParams _moveRangeParams;
        private RenderParams _pathParams;
        private RenderParams _abilityRangeParams;
        private RenderParams _abilityAreaOfEffectParams;
        private RenderParams _hoveredParams;

        private bool _renderBaseGrid = false;

        private Mesh _instancedMesh;

        private Material _instancedMaterial;
        private Material _selectedMaterial;
        private Material _neighborMaterial;
        private Material _moveRangeMaterial;
        private Material _pathMaterial;
        private Material _abilityRangeMaterial;
        private Material _abilityAreaOfEffectMaterial;
        private Material _hoveredMaterial;

        private List<Matrix4x4> _defaultRenders = new List<Matrix4x4>();
        private int _currentDefaultCount;

        public void AddInstance(TileData tileData)
        {
            if (_instancedTiles.ContainsKey(tileData.index))
            {
                if (_hoveredTile.index == tileData.index)
                {
                    _defaultRenders.Remove(_hoveredTile.tileMatrix);
                    _currentDefaultCount--;
                    _hoveredTile.tileMatrix = tileData.tileMatrix;
                }

                _instancedTiles[tileData.index] = tileData;
            }
            else
                _instancedTiles.TryAdd(tileData.index, tileData);
        }

        public void RemoveInstance(TileData tileData)
        {
            if (_instancedTiles.ContainsKey(tileData.index))
                _instancedTiles.Remove(tileData.index);
            if (_selectedTiles.Contains(tileData.tileMatrix))
                _selectedTiles.Remove(tileData.tileMatrix);

            if (tileData.tileStates != null)
            {
                if (tileData.tileStates.Contains(TileState.Hovered))
                {
                    _hoveredTile.tileMatrix = new Matrix4x4();
                }
            }
        }

        public void AddState(GridIndex index, TileState state)
        {
            if (state == TileState.Hovered)
            {
                if (_instancedTiles.TryGetValue(index, out TileData tileData))
                {
                    _hoveredTile = tileData;
                }
            }
            if (state == TileState.Selected)
            {
                _selectedTiles.Add(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsNeighbor)
            {
                _neighborTiles.Add(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsInMoveRange)
            {
                _moveRangeTiles.Add(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsInPath)
            {
                _pathTiles.Add(_instancedTiles[index].tileMatrix);
            }
            if(state == TileState.IsInAbilityRange)
            {
                _abilityRangeTiles.Add(_instancedTiles[index].tileMatrix);
            }
            if(state == TileState.IsInAoeRange)
            {
                _abilityAreaOfEffectTiles.Add(_instancedTiles[index].tileMatrix);
            }
        }

        public void RemoveState(GridIndex index, TileState state)
        {
            if (state == TileState.Hovered)
            {
                _hoveredTile = default(TileData);
            }
            if (state == TileState.Selected)
            {
                _selectedTiles.Remove(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsNeighbor)
            {
                _neighborTiles.Remove(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsInMoveRange)
            {
                _moveRangeTiles.Remove(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsInPath)
            {
                _pathTiles.Remove(_instancedTiles[index].tileMatrix);
            }
            if(state == TileState.IsInAbilityRange)
            {
                _abilityRangeTiles.Remove(_instancedTiles[index].tileMatrix);
            }
            if (state == TileState.IsInAoeRange)
            {
                _abilityAreaOfEffectTiles.Remove(_instancedTiles[index].tileMatrix);
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
                if (_renderBaseGrid)
                {
                    if (_currentDefaultCount != _instancedTiles.Count)
                    {
                        _defaultRenders.Clear();
                        for (int i = 0; i < _instancedTiles.Count; i++)
                        {
                            Matrix4x4 instanceMatrix = _instancedTiles.ElementAt(i).Value.tileMatrix;
                            _defaultRenders.Add(instanceMatrix);
                        }
                        _currentDefaultCount = _instancedTiles.Count;
                    }
                    Graphics.RenderMeshInstanced(_renderParams, _instancedMesh, 0, _defaultRenders);
                }

                if (_selectedTiles.Count > 0)
                {
                    Graphics.RenderMeshInstanced(_selectedParams, _instancedMesh, 0, _selectedTiles);
                }
                if (_neighborTiles.Count > 0)
                {
                    Graphics.RenderMeshInstanced(_neighborParams, _instancedMesh, 0, _neighborTiles);
                }
                if (_moveRangeTiles.Count > 0)
                {
                    Graphics.RenderMeshInstanced(_moveRangeParams, _instancedMesh, 0, _moveRangeTiles);
                }
                if (_pathTiles.Count > 0)
                {
                    Graphics.RenderMeshInstanced(_pathParams, _instancedMesh, 0, _pathTiles);
                }
                if (_abilityRangeTiles.Count > 0)
                {
                    Graphics.RenderMeshInstanced(_abilityRangeParams, _instancedMesh, 0, _abilityRangeTiles);
                }
                if (_abilityAreaOfEffectTiles.Count > 0)
                {
                    Graphics.RenderMeshInstanced(_abilityAreaOfEffectParams, _instancedMesh, 0, _abilityAreaOfEffectTiles);
                }
                if (!_hoveredTile.Equals(default(TileData)))
                {
                    Graphics.RenderMeshInstanced(_hoveredParams, _instancedMesh, 0, new[] { _hoveredTile.tileMatrix });
                }
            }
        }

        public void ClearInstances()
        {
            _instancedTiles.Clear();
            _selectedTiles.Clear();
            _neighborTiles.Clear();
            _pathTiles.Clear();
            _moveRangeTiles.Clear();
            _abilityRangeTiles.Clear();
            _abilityAreaOfEffectTiles.Clear();
            _hoveredTile = default(TileData);
        }

        public void UpdateGridMeshInstances(Mesh mesh, Material material, List<TileData> gridTiles)
        {
            ClearInstances();

            _instancedMesh = mesh;

            _instancedMaterial = new Material(material);
            _selectedMaterial = new Material(material);
            _neighborMaterial = new Material(material);
            _moveRangeMaterial = new Material(material);
            _pathMaterial = new Material(material);
            _abilityRangeMaterial = new Material(material);
            _abilityAreaOfEffectMaterial = new Material(material);
            _hoveredMaterial = new Material(material);

            _instancedMaterial.color = _instanceColor;
            _instancedMaterial.SetFloat("_IsFilled", 0f);
            _selectedMaterial.color = _selectedColor;
            _selectedMaterial.SetFloat("_IsFilled", 1f);
            _neighborMaterial.color = _neighborColor;
            _neighborMaterial.SetFloat("_IsFilled", 0.5f);
            _moveRangeMaterial.color = _moveRangeColor;
            _moveRangeMaterial.SetFloat("_IsFilled", 0.25f);
            _pathMaterial.color = _pathColor;
            _pathMaterial.SetFloat("_IsFilled", 1f);
            _abilityRangeMaterial.color = _abilityRangeColor;
            _abilityRangeMaterial.SetFloat("_IsFilled", 0.25f);
            _abilityAreaOfEffectMaterial.color = _abilityAreaOfEffectColor;
            _abilityAreaOfEffectMaterial.SetFloat("_IsFilled", 1.0f);
            _hoveredMaterial.color = _hoveredColor;
            _hoveredMaterial.SetFloat("_IsFilled", 0.5f);

            _renderParams = new RenderParams(_instancedMaterial);
            _selectedParams = new RenderParams(_selectedMaterial);
            _neighborParams = new RenderParams(_neighborMaterial);
            _moveRangeParams = new RenderParams(_moveRangeMaterial);
            _pathParams = new RenderParams(_pathMaterial);
            _abilityRangeParams = new RenderParams(_abilityRangeMaterial);
            _abilityAreaOfEffectParams = new RenderParams(_abilityAreaOfEffectMaterial);
            _hoveredParams = new RenderParams(_hoveredMaterial);

            gridTiles.ForEach(tile =>
            {
                _instancedTiles.Add(tile.index, tile);
            });

            _currentDefaultCount = 0;
        }
    }
}