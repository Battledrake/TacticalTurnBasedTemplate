using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private int xSize, zSize;

    private Vector3[] _vertices;
    private Mesh _mesh;

    private void Awake()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        this.GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Procedural Mesh";

        _vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector2[] uv = new Vector2[_vertices.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++, i++)
            {
                _vertices[i] = new Vector3(x, 0f, z) + this.transform.position;
                uv[i] = new Vector2(x / xSize, z / zSize);
            }
        }

        _mesh.vertices = _vertices;
        _mesh.uv = uv;

        int[] triangles = new int[xSize * zSize * 6];
        for(int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++)
        {
            for(int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }

        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (_vertices == null) return;

        Gizmos.color = Color.black;
        for (int i = 0; i < _vertices.Length; i++)
        {
            Gizmos.DrawSphere(_vertices[i], 0.1f);
        }
    }
}
