using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class DynamicAbility : Ability
    {
        protected IRangeDataSource _rangeDataSource;
        protected IAreaOfEffectDataSource _areaOfEffectDataSource;
        protected IEffectSource _effectSource;

        public void SetAbilitySources(IRangeDataSource rangeDataSource, IAreaOfEffectDataSource areaOfEffectDataSource, IEffectSource effectSource)
        {
            SetRangeDataSource(rangeDataSource);
            SetAreaOfEffectDataSource(areaOfEffectDataSource);
            SetEffectSource(effectSource);
        }

        public override AbilityRangeData GetRangeData() { return _rangeDataSource.GetRangeData(); }
        public override AbilityRangeData GetAreaOfEffectData() { return _areaOfEffectDataSource.GetAreaOfEffectData(); }
        public override List<AbilityEffect> GetEffects() { return _effectSource.GetEffects(); }

        public void SetRangeDataSource(IRangeDataSource rangeDataSource)
        {
            _rangeDataSource = rangeDataSource;
        }

        public void SetAreaOfEffectDataSource(IAreaOfEffectDataSource areaOfEffectDataSource)
        {
            _areaOfEffectDataSource = areaOfEffectDataSource;
        }

        public void SetEffectSource(IEffectSource effectSource)
        {
            _effectSource = effectSource;
        }

        public abstract override void ActivateAbility(AbilityActivationData activationData);
    }
}