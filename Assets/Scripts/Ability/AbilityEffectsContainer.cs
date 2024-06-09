using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CreateAssetMenu(fileName = "EffectsContainer", menuName = "TTBT/Ability/EffectsContainer")]
    public class AbilityEffectsContainer : ScriptableObject
    {
        public List<AbilityEffect> effects;
    }
}