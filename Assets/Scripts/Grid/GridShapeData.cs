using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EGridShape
{
    None,
    Square,
    Hexagon,
    Triangle
}

[CreateAssetMenu(fileName = "Data", menuName = "TTBTk/Grid/GridShapeData")]
public class GridShapeData : ScriptableObject
{
    public EGridShape _gridShape;
    public Vector3 _meshSize;
    public Mesh _mesh;
    public Material _material;
    public Mesh _flatMesh;
    public Material _flatBorderMaterial;
    public Material _flatFilledMaterial;
}
