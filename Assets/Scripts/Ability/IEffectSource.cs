using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public interface IEffectSource
    {
        public List<AbilityEffect> GetEffects();
    }
}