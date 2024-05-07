using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    [ExecuteInEditMode]
    public class GridMeshInstance : MonoBehaviour
    {
        private Dictionary<Vector2Int, Matrix4x4> _instancedTiles = new Dictionary<Vector2Int, Matrix4x4>();
        private Dictionary<Vector2Int, Matrix4x4> _selectedTiles = new Dictionary<Vector2Int, Matrix4x4>();
        private TileData _hoveredTile;
        private RenderParams _renderParams;
        private RenderParams _selectedParams;
        private RenderParams _hoveredParams;
        private Mesh _instancedMesh;
        private Material _instancedMaterial;
        private Material _selectedMaterial;
        private Material _hoveredMaterial;
        private Color _materialColor;

        public void AddInstance(TileData tileData)
        {
            /* NONE OF THIS WORKs RiGhT and LoOkS TeRrIbLe. REFACTOR!!! */
            //if (tileData.tileStates != null && tileData.tileStates.Count > 0)
            //{
            //    if (tileData.tileStates.Contains(TileState.Hovered))
            //    {
            //        _hoveredTile = tileData;
            //        if (tileData.tileStates.Contains(TileState.Selected))
            //        {
            //            _selectedTiles.TryAdd(tileData.index, _instancedTiles[tileData.index]);
            //            _instancedTiles.Remove(tileData.index);
            //        }
            //        if (_instancedTiles.ContainsKey(tileData.index))
            //        {
            //            _hoveredTile.tileMatrix = _instancedTiles[tileData.index]; //HACK: maintains groundoffset this way
            //            _instancedTiles.Remove(tileData.index);
            //        }
            //        else if (_selectedTiles.ContainsKey(tileData.index))
            //        {
            //            _hoveredTile.tileMatrix = _selectedTiles[tileData.index];
            //        }
            //    }
            //    else
            //    {
            //        if (tileData.tileStates.Contains(TileState.Selected))
            //        {
            //            _selectedTiles.TryAdd(tileData.index, tileData.tileMatrix);
            //        }
            //        else
            //        {
            //            _instancedTiles.TryAdd(tileData.index, tileData.tileMatrix);
            //        }
            //    }
            //}
            //else
            //{
            //    _instancedTiles.TryAdd(tileData.index, tileData.tileMatrix);
            //}
        }

        public void RemoveInstance(TileData tileData)
        {
            if (_instancedTiles.ContainsKey(tileData.index))
                _instancedTiles.Remove(tileData.index);
            if (_selectedTiles.ContainsKey(tileData.index))
                _selectedTiles.Remove(tileData.index);
        }

        //private Color GetColorFromState(HashSet<TileState> tileStates)
        //{
        //    if (tileStates != null && tileStates.Count > 0)
        //    {
        //        TileState[] priorityStates = { TileState.Selected, TileState.Hovered };
        //        foreach (var item in priorityStates)
        //        {
        //            if (tileStates.Contains(item))
        //            {
        //                switch (item)
        //                {
        //                    case TileState.Selected:
        //                        return new Color(255, 165, 0);
        //                    case TileState.Hovered:
        //                        return Color.yellow;
        //                }
        //            }
        //        }
        //    }
        //    return Color.black;
        //}

        public void UpdateGroundOffset(float offset)
        {
            for (int i = 0; i < _instancedTiles.Count; i++)
            {
                Vector2Int index = _instancedTiles.ElementAt(i).Key;
                Vector3 newPos = _instancedTiles[index].GetPosition();
                newPos.y += offset;
                _instancedTiles[index] = Matrix4x4.TRS(newPos, _instancedTiles[index].rotation, _instancedTiles[index].lossyScale);
            }
        }

        private void Update()
        {
            if (_instancedTiles.Count > 0)
            {
                Graphics.RenderMeshInstanced(_renderParams, _instancedMesh, 0, _instancedTiles.Values.ToList());
            }
            if (_selectedTiles.Count > 0)
            {
                Graphics.RenderMeshInstanced(_selectedParams, _instancedMesh, 0, _selectedTiles.Values.ToList());
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

            _renderParams = new RenderParams(_instancedMaterial);
            _selectedParams = new RenderParams(_selectedMaterial);
            _hoveredParams = new RenderParams(_hoveredMaterial);

            gridTiles.ForEach(tile =>
            {
                _instancedTiles.Add(tile.index, tile.tileMatrix);
            });
        }
    }
}