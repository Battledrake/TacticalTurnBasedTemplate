using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    //Temporary for building out system.
    public enum UnitId
    {
        Warrior,
        Ranger,
        Slime,
        Orc,
        Mech6,
        Bat,
        Drake,
        Cyborg
    }

    [CreateAssetMenu(fileName = "UnitData", menuName = "TTBT/Unit/UnitData")]
    public class UnitData : ScriptableObject
    {
        public UnitId unitId;
        public UnitAssetData assetData;
        public UnitStats unitStats;
    }
}