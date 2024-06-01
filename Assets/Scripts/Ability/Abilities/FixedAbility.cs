using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class FixedAbility : Ability
    {
        [Header("Ability Settings")]
        [SerializeField] protected AbilityRangeData _rangeData;
        [SerializeField] protected AbilityRangeData _areaOfEffectData;
        [SerializeField] protected List<AbilityEffect> _effects;

        public override AbilityRangeData GetAreaOfEffectData()
        {
            return _areaOfEffectData;
        }

        public override List<AbilityEffect> GetEffects()
        {
            return _effects;
        }

        public override AbilityRangeData GetRangeData()
        {
            return _rangeData;
        }

        public abstract override void ActivateAbility();

        public abstract override bool CanActivateAbility();

        public abstract override bool TryActivateAbility();

        protected abstract override void CommitAbility();
    }
}
