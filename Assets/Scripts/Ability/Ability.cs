using System;
using System.Collections.Generic;
using System.Linq;
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
        [Tooltip("End unit's turn when ability is used, regardless of remaining action points")]
        [SerializeField] protected bool _endTurnOnUse = true;
        [Tooltip("Ability effect used to determine cost of using ability")]
        [SerializeField] protected AbilityEffectsContainer _costEffect;
        [Tooltip("Turns before the ability can be used again")]
        [SerializeField] protected int _cooldown;
        [Tooltip("Ability only affects same team?")]
        [SerializeField] protected bool _isFriendlyOnly = false;
        public string Name => _abilityId.ToString();
        public Sprite Icon => _icon;

        public AbilityId AbilityId => _abilityId;
        public AbilitySystem AbilityOwner => _owner;
        public int ActiveCooldown => _activeCooldown;
        public bool EndTurnOnUse => _endTurnOnUse;
        public bool IsFriendlyOnly => _isFriendlyOnly;
        public abstract int UsesLeft { get; }
        public abstract AbilityRangeData RangeData { get; }
        public abstract AbilityRangeData AreaOfEffectData { get; }
        public abstract List<RangedAbilityEffect> Effects { get; }

        protected AbilitySystem _owner;
        protected int _activeCooldown;
        protected bool _cheatEnabled = false;

        public virtual void InitAbility(AbilitySystem abilitySystem)
        {
            _owner = abilitySystem;
        }
        public bool SetCheat(bool isEnabled) => _cheatEnabled = isEnabled;

        public void ReduceCooldown(int amount)
        {
            if (_activeCooldown > 0)
            {
                _activeCooldown += amount;
            }
        }

        public abstract void ReduceUsesLeft(int amount);

        public virtual bool CanActivateAbility(AbilityActivationData activationData)
        {
            if (_cheatEnabled) return true;

            if (UsesLeft == 0) return false;

            if (_activeCooldown > 0) return false;

            if (_owner.GetAttributeCurrentValue(AttributeId.ActionPoints) < _costEffect.effects.FirstOrDefault(e => e.attribute == AttributeId.ActionPoints).magnitude) return false;

            if (!CombatManager.Instance.GetAbilityRange(activationData.originIndex, this.RangeData).Contains(activationData.targetIndex)) return false;

            return true;
        }

        protected virtual void CommitAbility()
        {
            if (_cheatEnabled) return;

            if (UsesLeft > 0)
                ReduceUsesLeft(-1);

            for (int i = 0; i < _costEffect.effects.Count; i++)
            {
                _owner.ApplyEffect(_costEffect.effects[i]);
            }

            _activeCooldown = _cooldown;
        }

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