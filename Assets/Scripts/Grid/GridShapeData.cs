using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public enum GridShape
    {
        None,
        Square,
        Hexagon,
        Triangle
    }

    [CreateAssetMenu(fileName = "Data", menuName = "TTBTk/Grid/GridShapeData")]
    public class GridShapeData : ScriptableObject
    {
        public GridShape gridShape;
        public Vector3 meshSize;
        public Mesh mesh;
        public Material material;
        public Mesh flatMesh;
        public Material flatMaterial;
        public Material obstacleMaterial;
    }
}
