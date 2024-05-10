using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public enum TileState
    {
        None,
        Hovered,
        Selected,
        IsNeighbor,
        IsInPath
    }

    public struct TileData
    {
        public GridIndex index;
        public TileType tileType;
        public Matrix4x4 tileMatrix;
        public HashSet<TileState> tileStates;
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(GridMeshInstancer))]
    public class GridVisual : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GridMeshInstancer _gridMeshInstance;
        [SerializeField] private TacticsGrid _tacticsGrid;

        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public float GroundOffset { get => _groundOffset; set => SetOffsetFromGround(value); }

        private float _groundOffset = 0.1f;

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
            if (GridStatics.IsTileTypeWalkable(tileData.tileType))
            {
                Vector3 newPos = tileData.tileMatrix.GetPosition();
                newPos.y += _groundOffset;
                tileData.tileMatrix = Matrix4x4.TRS(newPos, tileData.tileMatrix.rotation, tileData.tileMatrix.lossyScale);
                _gridMeshInstance.AddInstance(tileData);
            }
            else
            {
                _gridMeshInstance.RemoveInstance(tileData);
            }
        }

        public void AddTileState(GridIndex index, TileState tileState)
        {
            _gridMeshInstance.AddState(index, tileState);
        }

        public void RemoveTileState(GridIndex index, TileState tileState)
        {
            _gridMeshInstance.RemoveState(index, tileState);
        }

        public void ClearGridVisual()
        {
            _gridMeshInstance.ClearInstances();
        }

        public void ClearPathVisual()
        {
            _gridMeshInstance.ClearPathVisual();
        }
    }
}
