using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    [CreateAssetMenu(fileName = "UnitData", menuName = "TTBT/Unit/UnitData")]
    public class UnitData : ScriptableObject
    {
        public UnitType unitType;
        public UnitAssetData assetData;
        public UnitStats unitStats;
    }
}