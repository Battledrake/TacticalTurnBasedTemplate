using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CreateAssetMenu(fileName = "New Armor", menuName = "TTBT/Data/Equipment/Armor")]
    public class Armor : ScriptableObject
    {
        public string armorName;
        public Sprite armorIcon;

        public List<GameplayEffect> unitEffects;
    }
}