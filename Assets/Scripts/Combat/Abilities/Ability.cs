using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

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

    [System.Serializable]
    public struct AbilityRangeData
    {
        public AbilityRangePattern rangePattern;
        public Vector2Int rangeMinMax;
        public LineOfSightData lineOfSightData;
    }

    [System.Serializable]
    public struct LineOfSightData
    {
        public bool requireLineOfSight;
        public float height;
    }

    public abstract class Ability : MonoBehaviour
    {
        public event Action<Ability> OnBehaviorComplete;

        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;

        [SerializeField] protected AbilityRangeData _rangeData;
        [SerializeField] protected AbilityRangeData _areaOfEffectData;

        public string Name { get => _name; }
        public Sprite Icon { get => _icon; }
        public AbilityRangeData RangeData { get => _rangeData; }
        public AbilityRangeData AreaOfEffectData { get => _areaOfEffectData; }

        protected GridIndex _originIndex;
        protected GridIndex _targetIndex;
        protected List<GridIndex> _aoeIndexes;

        protected TacticsGrid _tacticsGrid;


        public void InitializeAbility(TacticsGrid tacticsGrid, GridIndex originIndex, GridIndex targetIndex)
        {
            _tacticsGrid = tacticsGrid;
            _originIndex = originIndex;
            _targetIndex = targetIndex;
        }

        public void InitializeAbility(TacticsGrid tacticsGrid, GridIndex originIndex, GridIndex targetIndex, List<GridIndex> aoeIndexes)
        {
            _tacticsGrid = tacticsGrid;
            _originIndex = originIndex;
            _targetIndex = targetIndex;
            _aoeIndexes = aoeIndexes;
        }

        protected void AbilityBehaviorComplete(Ability ability)
        {
            OnBehaviorComplete?.Invoke(ability);
        }

        public abstract bool CanActivateAbility();

        protected abstract void CommitAbility();

        public abstract void ActivateAbility();

        public abstract bool TryActivateAbility();
        public abstract void EndAbility();
    }
}