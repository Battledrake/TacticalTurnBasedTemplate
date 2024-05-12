using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum GridShape
    {
        None,
        Square,
        Hexagon,
        Triangle
    }

    [CreateAssetMenu(fileName = "GridData", menuName = "TTBT/Grid/GridShapeData")]
    public class GridShapeData : ScriptableObject
    {
        public GridShape gridShape;
        public Vector3 meshSize;
        public Mesh mesh;
        public Material material;
        public Mesh flatMesh;
        public Material flatMaterial;
        public Material obstacleMaterial;
        public Material doubleCostMaterial;
        public Material tripleCostMaterial;
        public Material flyingOnlyMaterial;
    }
}
