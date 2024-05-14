using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CreateAssetMenu(fileName = "AssetData", menuName = "TTBT/Unit/UnitAssetData")]
    public class UnitAssetData : ScriptableObject
    {
        public Sprite unitIcon;
        public GameObject unitVisual;
    }
}