using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class FixedAbility : Ability
    {
        [Header("Fixed Ability Attributes")]
        [Tooltip("Number of times this ability can be used in a single combat iteration. -1 is unlimited")]
        [SerializeField] protected int _usesPerCombat = -1;
        
        [SerializeField] protected AbilityRangeData _rangeData;
        [SerializeField] protected AbilityRangeData _areaOfEffectData;
        [SerializeField] protected List<RangedAbilityEffect> _effects;

        protected int _abilityUsesLeft = -1;

        public override void InitAbility(AbilitySystem abilitySystem)
        {
            base.InitAbility(abilitySystem);
            if(_usesPerCombat > -1)
            {
                _abilityUsesLeft = _usesPerCombat;
            }
        }

        public override int GetUsesLeft()
        {
            return _abilityUsesLeft;
        }

        public override void ReduceUsesLeft(int amount)
        {
            _abilityUsesLeft += amount;
        }

        public override AbilityRangeData GetAreaOfEffectData()
        {
            return _areaOfEffectData;
        }

        public override List<RangedAbilityEffect> GetEffects()
        {
            return _effects;
        }

        public override AbilityRangeData GetRangeData()
        {
            return _rangeData;
        }

        public abstract override void ActivateAbility(AbilityActivationData activationData);
    }
}
