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
        public int magnitude { get; private set; }
        public int duration { get; private set; }
        public int baseInterval { get; private set; }
        public int currentInterval { get; private set; }
        /// <summary>
        /// This is set based on whether we assign a period interval value in the inspector. If 0, we set to false. Used for effect application checks.
        /// </summary>
        public bool isPeriodic { get; private set; }

        public ActiveEffect(EffectDurationPolicy durationPolicy, AttributeId attribute, int magnitude, int duration, int interval)
        {
            this.durationPolicy = durationPolicy;
            this.attribute = attribute;
            this.magnitude = magnitude;
            this.duration = duration;
            if (interval == 0)
                isPeriodic = false;
            else
                isPeriodic = true;

            baseInterval = interval;
            currentInterval = baseInterval;
        }
        public void UpdateDuration(int amount)
        {
            duration += amount;
        }

        public void UpdateInterval(int amount)
        {
            currentInterval += amount;
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
        Infinite
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
        [Tooltip("Application Interval (in turns). Effect magnitude is applied to base value on each interval. 0 disables periodic effect.")]
        public int interval;
        [Tooltip("Apply effect magnitude immediately or wait until interval count")]
        public bool executeImmediately;
    }

    [Serializable]
    public struct RangedAbilityEffect
    {
        public EffectDurationData durationData;
        public AttributeId attribute;
        public Vector2Int magnitudeRange;
    }

    [Serializable]
    public struct AbilityEffect
    {
        public EffectDurationData durationData;
        public AttributeId attribute;
        public int magnitude;
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
        protected int _activeCooldown;

        public AbilityId GetAbilityId() => _abilityId;
        public AbilitySystem GetAbilityOwner() => _owner;
        public int GetActiveCooldown() => _activeCooldown;

        public abstract AbilityRangeData GetRangeData();
        public abstract AbilityRangeData GetAreaOfEffectData();
        public abstract List<RangedAbilityEffect> GetEffects();

        public void InitAbility(AbilitySystem abilitySystem)
        {
            _owner = abilitySystem;
        }

        public void ReduceCooldown(int amount)
        {
            if (_activeCooldown > 0)
            {
                _activeCooldown += amount;
            }
        }

        public virtual bool CanActivateAbility(AbilityActivationData activationData)
        {
            if (_activeCooldown > 0) return false;

            if (_owner.GetAttributeCurrentValue(AttributeId.ActionPoints) <= 0) return false;

            if (CombatManager.Instance.GetAbilityRange(activationData.originIndex, this.GetRangeData()).Contains(activationData.targetIndex)) return true;

            return false;
        }

        protected virtual void CommitAbility() { _owner.ApplyEffect(_costEffect.effect); _activeCooldown = _cooldown; }

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