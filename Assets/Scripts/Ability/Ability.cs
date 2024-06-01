using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AbilityId
    {
        MeleeHit,
        SummonBat,
        VineAssault,
        WindGust,
        BladeStorm,
        DarkWave,
        Heal,
        LongRangeArrow,
        RunChickenRun,
        ShortRangeArrow,
        SlimeBall,
        SwordSlash,
        SwordThrow
    }
    public enum AbilityRangePattern
    {
        None,
        Line,
        Diagonal,
        HalfDiagonal,
        Star,
        Diamond,
        Square
    }

    public struct AbilityActivationData
    {
        public TacticsGrid tacticsGrid;
        public GridIndex originIndex;
        public GridIndex targetIndex;
    }

    [Serializable]
    public struct AbilityRangeData
    {
        public AbilityRangePattern rangePattern;
        public Vector2Int rangeMinMax;
        public LineOfSightData lineOfSightData;
    }

    [Serializable]
    public struct LineOfSightData
    {
        /// <summary>
        /// Validate target with line of sight raytrace.
        /// </summary>
        public bool requireLineOfSight;
        /// <summary>
        /// Height tile floor to raycast from.
        /// </summary>
        public float height;
        /// <summary>
        /// If greater than 0, % distance from center to edge for Cardinal-based offset checks.
        /// </summary>
        [Range(0.0f, 1f)]
        public float offsetDistance;
    }

    public enum EffectType
    {
        Instant,
        Duration,
        Permanent
    }

    [Serializable]
    public struct AbilityEffect
    {
        public AttributeType attributeType;
        public Vector2Int minMaxModifier;
    }

    public struct AbilityEffectReal
    {
        public AttributeType attributeType;
        public int modifier;
    }

    public abstract class Ability : MonoBehaviour
    {
        public event Action<Ability> OnAbilityEnded;

        [Header("Ability")]

        [Header("Ability Asset Data")]
        [SerializeField] private AbilityId _abilityId;
        [SerializeField] protected Sprite _icon;

        [Header("Ability Attributes")]
        [SerializeField] protected int _abilityCost;
        [SerializeField] protected bool _isFriendly = false;

        public bool IsFriendly { get => _isFriendly; }

        public string Name { get => _abilityId.ToString(); }
        public Sprite Icon { get => _icon; }
        public int AbilityCost { get => _abilityCost; }
        public Unit Instigator { get => _instigator; }

        protected Unit _instigator;

        protected AbilitySystem _owner;

        public AbilityId GetAbilityId()
        {
            return _abilityId;
        }
        public AbilitySystem GetAbilityOwner() { return _owner; }

        public abstract AbilityRangeData GetRangeData();
        public abstract AbilityRangeData GetAreaOfEffectData();
        public abstract List<AbilityEffect> GetEffects();

        public void InitAbility(AbilitySystem abilitySystem)
        {
            _owner = abilitySystem;
        }

        public virtual bool CanActivateAbility(AbilityActivationData activateData)
        {
            if (_owner.CurrentAbilityPoints <= 0)
                return false;

            if (CombatManager.Instance.GetAbilityRange(activateData.originIndex, this.GetRangeData()).Contains(activateData.targetIndex))
                return true;
            return false;
        }

        protected virtual void CommitAbility() { _owner.RemoveAbilityPoints(_abilityCost); }

        public abstract void ActivateAbility(AbilityActivationData activationData);

        public virtual bool TryActivateAbility(AbilityActivationData activationData)
        {
            if (CanActivateAbility(activationData))
            {
                ActivateAbility(activationData);
                return true;
            }
            else
                return false;
        }
        public virtual void EndAbility()
        {
            OnAbilityEnded?.Invoke(this);
        }
    }
}