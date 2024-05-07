using BattleDrakeCreations.TTBTk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public enum TileState
    {
        None,
        Hovered,
        Selected
    }

    public struct TileData
    {
        public Vector2Int index;
        public TileType tileType;
        public Matrix4x4 tileMatrix;
        public HashSet<TileState> tileStates;
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

        public void UpdateGridVisual(GridShapeData gridShapeData, List<TileData> gridTiles)
        {
            ClearGridVisual();
            _gridMeshInstance.UpdateGridMeshInstances(gridShapeData.flatMesh, gridShapeData.flatMaterial, Color.black, gridTiles);
            SetOffsetFromGround(_groundOffset);
        }

        public void UpdateTileVisual(TileData tileData)
        {
            _gridMeshInstance.AddInstance(tileData);
        }

        public void ClearGridVisual()
        {
            _gridMeshInstance.ClearInstances();
        }
    }
}
