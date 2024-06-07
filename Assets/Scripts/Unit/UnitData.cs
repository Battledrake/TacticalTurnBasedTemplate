using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum UnitId
    {
        Warrior,
        Ranger,
        Slime,
        Orc,
        Mech6,
        Bat,
        Drake,
        Cyborg,
        Pacca1,
        Pacca2,
        Rhino1,
        Rhino2
    }

    [CreateAssetMenu(fileName = "UnitData", menuName = "TTBT/Unit/NewUnit")]
    public class UnitData : ScriptableObject
    {
        public UnitId unitId;
        public UnitAssetData assetData;
        public UnitStats unitStats;
    }
}