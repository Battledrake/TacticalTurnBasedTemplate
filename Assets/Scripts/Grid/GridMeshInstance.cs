using BattleDrakeCreations.TTBTk;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GridMeshInstance : MonoBehaviour
{
    private List<Matrix4x4> _instancedTiles = new List<Matrix4x4>();
    //TODO: Add more Lists for different 'types' of tiles (really just tiles with different material properties).
    private RenderParams _renderParams;
    private Mesh _instancedMesh;
    private Material _instancedMaterial;
    private Color _materialColor;

    public void AddInstance(Matrix4x4 instanceMatrix)
    {
        _instancedTiles.Add(instanceMatrix);
    }

    public void RemoveInstance(Matrix4x4 instanceMatrix)
    {
        _instancedTiles.Remove(instanceMatrix);
    }

    public void UpdateGroundOffset(float offset)
    {
        for(int i = 0; i < _instancedTiles.Count; i++)
        {
            Vector3 newPos = _instancedTiles[i].GetPosition();
            newPos.y += offset;
            _instancedTiles[i] = Matrix4x4.TRS(newPos, _instancedTiles[i].rotation, _instancedTiles[i].lossyScale);
        }
    }

    private void Update()
    {
        if (_instancedTiles.Count > 0)
        {
            Graphics.RenderMeshInstanced(_renderParams, _instancedMesh, 0, _instancedTiles);
        }

    }

    public void ClearInstances()
    {
        _instancedTiles.Clear();
    }

    public void UpdateGridMeshInstances(Mesh mesh, Material material, Color color, List<Matrix4x4> gridTiles)
    {
        _instancedMesh = mesh;
        _instancedMaterial = new Material(material);
        _materialColor = color;
        _instancedMaterial.color = _materialColor;
        _renderParams = new RenderParams(_instancedMaterial);
        _instancedTiles = gridTiles;
    }
}