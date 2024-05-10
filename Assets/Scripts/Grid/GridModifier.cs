using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class GridModifier : MonoBehaviour
    {
        [SerializeField] private GridShape _gridShape;
        [SerializeField] private TileType _tileType;

        public TileType TileType { get => _tileType; }

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private GridShapeData _currentShapeData;

        private void Awake()
        {
        }

        private void OnValidate()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            if (_gridShape != GridShape.None)
            {
                _currentShapeData = GridStatics.GetGridShapeData(_gridShape);

                _meshFilter.mesh = _currentShapeData.mesh;
                _meshRenderer.material = _currentShapeData.obstacleMaterial;
                _meshCollider.sharedMesh = _currentShapeData.mesh;
                _meshCollider.convex = true;
            }
        }
    }
}
