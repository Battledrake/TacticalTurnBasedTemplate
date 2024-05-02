using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    [ExecuteInEditMode]
    public class TacticalGrid : MonoBehaviour
    {
        [SerializeField] private GridShape _gridShapeToggle = GridShape.Square;
        [SerializeField] private List<GridShapeData> _gridShapeData;
        [SerializeField] private Mesh _instancedMesh;
        [SerializeField] private Material _instancedMaterial;

        [SerializeField] private int _gridWidth = 5;
        [SerializeField] private int _gridHeight = 5;

        public int InstanceCount => _gridWidth * _gridHeight;

        private Vector3 _gridPosition;
        private Vector3 _tileSize;
        private Vector2Int _tileCount;
        private GridShape _gridShape;

        private void Awake()
        {
            SpawnGrid(Vector3.zero, Vector3.one, new Vector2Int(_gridWidth, _gridHeight), _gridShapeToggle);
        }

        private void Update()
        {
            if (_gridShapeToggle != _gridShape)
            {
                SpawnGrid(Vector3.zero, Vector3.one, new Vector2Int(_gridWidth, _gridHeight), _gridShapeToggle);
                return;
            }

            RenderParams renderParams = new RenderParams(_instancedMaterial);
            Matrix4x4[] instanceData = new Matrix4x4[_gridWidth * _gridHeight];
            int instanceIndex = 0;
            for (int z = 0; z < _gridHeight; ++z)
            {
                for (int x = 0; x < _gridWidth; ++x)
                {
                    Vector3 instancePosition = new Vector3(this.transform.position.x + + x * _tileSize.x, this.transform.position.y, this.transform.position.z + z * _tileSize.z);
                    Quaternion instanceRotation = Quaternion.Euler(-90.0f, 0.0f, 90.0f); //Imported meshes are using Unreal Coordinate System. Adjusting for Unity.
                    Vector3 instanceScale = _instancedMesh.bounds.size;
                    instanceData[instanceIndex] = Matrix4x4.TRS(instancePosition, instanceRotation, instanceScale);
                    instanceIndex++;
                }
            }
            if (instanceIndex > 0)
                Graphics.RenderMeshInstanced(renderParams, _instancedMesh, 0, instanceData);
        }

        public void SpawnGrid(Vector3 gridPosition, Vector3 tileSize, Vector2Int tileCount, GridShape gridShape)
        {
            _gridPosition = gridPosition;
            _tileSize = tileSize;
            _tileCount = tileCount;
            _gridShape = gridShape;

            GridShapeData activeData = _gridShapeData.Find(data => data.gridShape == gridShape);
            _instancedMesh = activeData.flatMesh;
            _instancedMaterial = activeData.flatFilledMaterial;
        }

        public GridShapeData GetCurrentShapeData()
        {
            return _gridShapeData[(int)_gridShapeToggle];
        }
    }
}
