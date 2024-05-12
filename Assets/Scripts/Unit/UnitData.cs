using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CreateAssetMenu(fileName = "Data", menuName = "TTBT/Unit/UnitData")]
    public class UnitData : ScriptableObject
    {
        public UnitType unitType;
        public string unitName;
        public Sprite unitIcon;
        public GameObject unitVisual;
    }
}