using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CreateAssetMenu(fileName = "EffectsContainer", menuName = "TTBT/Data/EffectsContainer")]
    public class GameplayEffectsContainer : ScriptableObject
    {
        public List<GameplayEffect> effects;
    }
}