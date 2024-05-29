using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AbilityId
    {
        Movement,
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
        public bool requireLineOfSight;
        public float height;
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
        public event Action OnAbilityEnd;
        public event Action<Ability> OnBehaviorComplete;

        [Header("Ability")]

        [Header("Ability Asset Data")]
        [SerializeField] private AbilityId _abilityId;
        [SerializeField] protected Sprite _icon;

        [Header("Ability Attributes")]
        [SerializeField] protected AbilityRangeData _rangeData;
        [SerializeField] protected AbilityRangeData _areaOfEffectData;
        [SerializeField] protected List<AbilityEffect> _effects;
        [SerializeField] protected int _abilityCost;
        [SerializeField] protected bool _isFriendly = false;

        public bool IsFriendly { get => _isFriendly; }

        public string Name { get => _abilityId.ToString(); }
        public Sprite Icon { get => _icon; }
        public AbilityRangeData RangeData { get => _rangeData; }
        public AbilityRangeData AreaOfEffectData { get => _areaOfEffectData; }
        public List<AbilityEffect> Effects { get => _effects; }
        public int AbilityCost { get => _abilityCost; }
        public Unit Instigator { get => _instigator; }

        protected Unit _instigator;

        protected GridIndex _originIndex;
        protected GridIndex _targetIndex;
        protected List<GridIndex> _aoeIndexes = new List<GridIndex>();

        protected TacticsGrid _tacticsGrid;

        public AbilityId GetAbilityId()
        {
            return _abilityId;
        }

        public void InitializeAbility(TacticsGrid tacticsGrid, Unit instigator, GridIndex originIndex, GridIndex targetIndex)
        {
            _tacticsGrid = tacticsGrid;
            _instigator = instigator;
            _originIndex = originIndex;
            _targetIndex = targetIndex;
        }

        public void InitializeAbility(TacticsGrid tacticsGrid, Unit instigator, GridIndex originIndex, GridIndex targetIndex, List<GridIndex> aoeIndexes)
        {
            _tacticsGrid = tacticsGrid;
            _instigator = instigator;
            _originIndex = originIndex;
            _targetIndex = targetIndex;
            _aoeIndexes = aoeIndexes;
        }

        protected void AbilityBehaviorComplete(Ability ability)
        {
            //TODO: No apparent use now, but will be used for turn system to allow ability to exist passed a single turn but still notify that it's finished that turn's behavior.
            OnBehaviorComplete?.Invoke(ability);
        }

        public abstract bool CanActivateAbility();

        protected abstract void CommitAbility();

        public abstract void ActivateAbility();

        public abstract bool TryActivateAbility();
        public virtual void EndAbility()
        {
            OnAbilityEnd?.Invoke();
            Destroy(this.gameObject); 
        }
    }
}