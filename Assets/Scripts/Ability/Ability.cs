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
        SwordThrow,
        ShootRifle,
        RainbowWave,
        RainbowSpin,
        RainbowBlast,
        KillUnit
    }
    public enum AbilityRangePattern
    {
        None,
        Line,
        Diagonal,
        HalfDiagonal,
        Star,
        Diamond,
        Square,
        Movement
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

    public class ActiveEffect
    {
        public EffectDurationPolicy durationPolicy { get; private set; }
        public AttributeId attribute { get; private set; }
        public int modifier { get; private set; }
        public int duration { get; private set; }
        public int baseInterval { get; private set; }
        public int currentInterval { get; private set; }
        /// <summary>
        /// This is set based on whether we assign a period interval value in the inspector. If 0, we set to false. Used for effect application checks.
        /// </summary>
        public bool isPeriodic { get; private set; }

        public ActiveEffect(EffectDurationPolicy durationPolicy, AttributeId attribute, int modifier, int duration, int interval)
        {
            this.durationPolicy = durationPolicy;
            this.attribute = attribute;
            this.modifier = modifier;
            this.duration = duration;
            if (interval == 0)
                isPeriodic = false;
            else
                isPeriodic = true;

            baseInterval = interval;
            currentInterval = baseInterval;
        }
        public void UpdateDuration(int modifier)
        {
            duration += modifier;
        }

        public void UpdateInterval(int modifier)
        {
            currentInterval += modifier;
        }

        public void ResetInterval()
        {
            currentInterval = baseInterval;
        }
    }

    public enum EffectDurationPolicy
    {
        Instant,
        Duration,
        Permanent
    }

    [Serializable]
    public struct EffectDurationData
    {
        public EffectDurationPolicy durationPolicy;
        public int duration;
        public EffectPeriodData period;
    }

    [Serializable]
    public struct EffectPeriodData
    {
        [Tooltip("Application Interval (in turns). 0 applies effect modifier to current value until duration expires.")]
        public int interval;
        [Tooltip("Apply effect modifier immediately or first on turn start")]
        public bool executeImmediately;
    }

    [Serializable]
    public struct AbilityEffect
    {
        public EffectDurationData durationData;
        public AttributeId attribute;
        public Vector2Int minMaxModifier;
    }

    [Serializable]
    public struct AbilityEffectReal
    {
        public EffectDurationData durationData;
        public AttributeId attribute;
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
        [SerializeField] protected AbilityEffectScriptable _costEffect;
        [SerializeField] protected int _cooldown;
        [SerializeField] protected bool _affectsFriendly = false;

        public bool AffectsFriendly { get => _affectsFriendly; }
        public string Name { get => _abilityId.ToString(); }
        public Sprite Icon { get => _icon; }

        protected AbilitySystem _owner;

        public AbilityId GetAbilityId() => _abilityId;
        public AbilitySystem GetAbilityOwner() => _owner;

        public abstract AbilityRangeData GetRangeData();
        public abstract AbilityRangeData GetAreaOfEffectData();
        public abstract List<AbilityEffect> GetEffects();

        public void InitAbility(AbilitySystem abilitySystem)
        {
            _owner = abilitySystem;
        }

        public void ReduceCooldown(int amount)
        {
            if (_cooldown > 0)
            {
                _cooldown -= amount;
                if (_cooldown < 0)
                    _cooldown = 0;
            }
        }

        public virtual bool CanActivateAbility(AbilityActivationData activationData)
        {
            if (_owner.GetAttributeCurrentValue(AttributeId.ActionPoints) <= 0)
                return false;

            if (CombatManager.Instance.GetAbilityRange(activationData.originIndex, this.GetRangeData()).Contains(activationData.targetIndex))
                return true;

            return false;
        }

        protected virtual void CommitAbility() { _owner.ApplyEffect(_costEffect.effect); }

        public abstract void ActivateAbility(AbilityActivationData activationData);

        public virtual bool TryActivateAbility(AbilityActivationData activationData)
        {
            if (CanActivateAbility(activationData))
            {
                ActivateAbility(activationData);
                return true;
            }
            else
            {
                return false;
            }

        }
        public virtual void EndAbility()
        {
            OnAbilityEnded?.Invoke(this);
        }
    }
}