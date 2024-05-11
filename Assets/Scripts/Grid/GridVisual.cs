using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
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
        [SerializeField] private GridMeshInstancer _gridMeshInstancer;
        [SerializeField] private TacticalMeshInstancer _tacticalMeshInstancer;
        [SerializeField] private TacticsGrid _tacticsGrid;

        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public float GroundOffset { get => _groundOffset; set => SetOffsetFromGround(value); }

        private float _groundOffset = 0.1f;

        public void HideDefaultGrid()
        {
            _gridMeshInstancer.enabled = false;
        }

        public void ShowDefaultGrid()
        {
            _gridMeshInstancer.enabled = true;
        }
        public void HideTacticalGrid()
        {
            _tacticalMeshInstancer.enabled = false;
        }
        public void ShowTacticalGrid()
        {
            _tacticalMeshInstancer.enabled = true;
        }

        private void SetOffsetFromGround(float value)
        {
            _groundOffset = value;
            if (_tacticsGrid.UseEnvironment)
            {
                _gridMeshInstancer.UpdateGroundOffset(_groundOffset);
                _tacticalMeshInstancer.UpdateGroundOffset(_groundOffset);
            }

        }

        public void UpdateGridVisual(GridShapeData gridShapeData, List<TileData> gridTiles)
        {
            ClearGridVisual();
            _gridMeshInstancer.UpdateGridMeshInstances(gridShapeData.flatMesh, gridShapeData.flatMaterial, gridTiles);
            _tacticalMeshInstancer.UpdateGridMeshInstances(gridShapeData.mesh, gridShapeData.material, gridTiles);
            SetOffsetFromGround(_groundOffset);

            HideTacticalGrid();
        }

        public void UpdateTileVisual(TileData tileData)
        {
            if (!GridStatics.IsTileTypeWalkable(tileData.tileType))
            {
                if (tileData.tileType == TileType.None)
                {
                    _tacticalMeshInstancer.RemoveInstance(tileData);
                }
                _gridMeshInstancer.RemoveInstance(tileData);
            }
            else
            {
                Vector3 newPos = tileData.tileMatrix.GetPosition();
                newPos.y += _groundOffset;
                tileData.tileMatrix = Matrix4x4.TRS(newPos, tileData.tileMatrix.rotation, tileData.tileMatrix.lossyScale);
                _gridMeshInstancer.AddInstance(tileData);
                _tacticalMeshInstancer.AddInstance(tileData);
            }
        }

        public void AddTileState(GridIndex index, TileState tileState)
        {
            _gridMeshInstancer.AddState(index, tileState);
        }

        public void RemoveTileState(GridIndex index, TileState tileState)
        {
            _gridMeshInstancer.RemoveState(index, tileState);
        }

        public void ClearGridVisual()
        {
            _gridMeshInstancer.ClearInstances();
            _tacticalMeshInstancer.ClearInstances();
        }

        public void ClearPathVisual()
        {
            _gridMeshInstancer.ClearPathVisual();
        }
    }
}
