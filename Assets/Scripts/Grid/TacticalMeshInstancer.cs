using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TacticalMeshInstancer : MonoBehaviour
    {
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _doubleColor = Color.yellow;
        [SerializeField] private Color _tripleColor = Color.magenta;
        [SerializeField] private Color _flyingColor = Color.blue;
        [SerializeField] private Color _obstacleColor = Color.red;

        private Mesh _instancedMesh;
        private Dictionary<GridIndex, TileData> _instancedTiles = new Dictionary<GridIndex, TileData>();
        private Dictionary<TileType, List<Matrix4x4>> _tileTypeRenders = new Dictionary<TileType, List<Matrix4x4>>();
        private Dictionary<TileType, RenderParams> _renderParams = new Dictionary<TileType, RenderParams>();
        private Dictionary<TileType, Material> _tileMaterials = new Dictionary<TileType, Material>();

        public void AddInstance(TileData tileData)
        {
            if (_instancedTiles.TryGetValue(tileData.index, out TileData prevTileData))
            {
                _tileTypeRenders[prevTileData.tileType].Remove(prevTileData.tileMatrix);
                _tileTypeRenders[tileData.tileType].Add(tileData.tileMatrix);
                _instancedTiles[tileData.index] = tileData;
            }
            else
            {
                if (_instancedTiles.TryAdd(tileData.index, tileData))
                {
                    if (_tileTypeRenders.ContainsKey(tileData.tileType))
                        _tileTypeRenders[tileData.tileType].Add(tileData.tileMatrix);
                    else
                        _tileTypeRenders.Add(tileData.tileType, new List<Matrix4x4> { tileData.tileMatrix });
                }
            }
        }

        public void RemoveInstance(TileData tileData)
        {
            if (_instancedTiles.TryGetValue(tileData.index, out TileData instancedData))
            {
                _tileTypeRenders[instancedData.tileType].Remove(instancedData.tileMatrix);
                _instancedTiles.Remove(tileData.index);
            }
        }

        public void UpdateGroundOffset(float offset)
        {
            for (int i = 0; i < _tileTypeRenders.Count; i++)
            {
                _tileTypeRenders.ElementAt(i).Value.Clear();
            }

            for (int i = 0; i < _instancedTiles.Count; i++)
            {
                TileData tileData = _instancedTiles.ElementAt(i).Value;
                Vector3 newPos = tileData.tileMatrix.GetPosition();
                newPos.y += offset;
                tileData.tileMatrix = Matrix4x4.TRS(newPos, tileData.tileMatrix.rotation, tileData.tileMatrix.lossyScale);
                _instancedTiles[tileData.index] = tileData;

                _tileTypeRenders[tileData.tileType].Add(tileData.tileMatrix);
            }
        }

        private void Update()
        {
            if (_tileTypeRenders.Count > 0)
            {
                for (int i = 0; i < _tileTypeRenders.Count; i++)
                {
                    if (_tileTypeRenders.ElementAt(i).Value.Count > 0)
                    {
                        Graphics.RenderMeshInstanced(_renderParams[(TileType)_tileTypeRenders.ElementAt(i).Key], _instancedMesh, 0, _tileTypeRenders.ElementAt(i).Value);
                    }
                }
            }
        }

        public void ClearInstances()
        {
            _instancedTiles.Clear();
            for (int i = 0; i < _tileTypeRenders.Count; i++)
            {
                _tileTypeRenders.ElementAt(i).Value.Clear();
            }
        }

        public void UpdateGridMeshInstances(Mesh mesh, Material material, List<TileData> gridTiles)
        {
            ClearInstances();

            _instancedMesh = mesh;

            List<TileType> tileTypeList = Enum.GetValues(typeof(TileType)).Cast<TileType>().ToList();
            tileTypeList.Remove(TileType.None);

            tileTypeList.ForEach(tt =>
            {
                _tileMaterials[tt] = new Material(material);
                _tileMaterials[tt].color = GetColorFromTileType(tt);
                _renderParams[tt] = new RenderParams(_tileMaterials[tt]);
                _tileTypeRenders[tt] = new List<Matrix4x4>();
            });

            gridTiles.ForEach(tile =>
            {
                _instancedTiles.Add(tile.index, tile);

                _tileTypeRenders.TryGetValue(tile.tileType, out List<Matrix4x4> renderMatrix);
                renderMatrix.Add(tile.tileMatrix);
            });
        }

        private Color GetColorFromTileType(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Normal:
                    return _normalColor;
                case TileType.DoubleCost:
                    return _doubleColor;
                case TileType.TripleCost:
                    return _tripleColor;
                case TileType.FlyingOnly:
                    return _flyingColor;
                case TileType.Obstacle:
                    return _obstacleColor;
            }
            return Color.clear;
        }
    }
}