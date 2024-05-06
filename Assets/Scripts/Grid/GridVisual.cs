using BattleDrakeCreations.TTBTk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public struct TileData
    {
        public Vector2Int index;
        public TileType tileType;
        public Matrix4x4 tileMatrix;
    }

    public struct TileTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(GridMeshInstance))]
    public class GridVisual : MonoBehaviour
    {
        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public float GroundOffset { get => _groundOffset; set => SetOffsetFromGround(value); }

        private GridMeshInstance _gridMeshInstance;
        private TacticsGrid _tacticsGrid; //No use currently but might be nice to have for future debugging
        private float _groundOffset = 0.1f;

        private void Awake()
        {
            _tacticsGrid = this.GetComponent<TacticsGrid>();
            _gridMeshInstance = this.GetComponent<GridMeshInstance>();
        }

        private void SetOffsetFromGround(float value)
        {
            _groundOffset = value;
            if (_tacticsGrid.UseEnvironment)
                _gridMeshInstance.UpdateGroundOffset(_groundOffset);
        }

        public void UpdateGridVisual(GridShapeData gridShapeData, List<Matrix4x4> tileTransforms)
        {
            ClearGridVisual();
            _gridMeshInstance.UpdateGridMeshInstances(gridShapeData.flatMesh, gridShapeData.flatBorderMaterial, Color.black, tileTransforms);
            SetOffsetFromGround(_groundOffset);
        }

        public void UpdateTileVisual(TileData tileData)
        {
            //_gridMeshInstance.RemoveInstance(tileData.index);
            //if (GridStatics.IsTileTypeWalkable(tileData.tileType))
            //{
            //    tileData.position.y += _groundOffset;
            //    _gridMeshInstance.AddInstance(tileData);
            //}
        }

        public void ClearGridVisual()
        {
            _gridMeshInstance.ClearInstances();
        }
    }
}
