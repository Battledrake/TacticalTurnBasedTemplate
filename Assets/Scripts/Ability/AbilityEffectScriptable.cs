using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CreateAssetMenu(fileName = "Effect", menuName = "TTBT/Ability/Effect")]
    public class AbilityEffectScriptable : ScriptableObject
    {
        public AbilityEffectReal effect;
    }
}