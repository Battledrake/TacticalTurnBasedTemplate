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
    }

    public abstract class Ability : MonoBehaviour
    {
        
        [SerializeField] protected AbilityRangePattern _rangePattern;
        [SerializeField] protected AbilityRangeData _toTargetData;
        [SerializeField] protected AbilityRangeData _onTargetData;
        [SerializeField] protected bool _requireLineOfSight;

        public AbilityRangeData ToTargetData { get => _toTargetData; }
        public AbilityRangeData OnTargetData { get => _onTargetData; }

        protected GridIndex _originIndex;
        protected GridIndex _targetIndex;

        protected TacticsGrid _tacticsGrid;


        public void InitializeAbility(TacticsGrid tacticsGrid, GridIndex origin, GridIndex target)
        {
            _tacticsGrid = tacticsGrid;
            _originIndex = origin;
            _targetIndex = target;
        }

        public abstract bool CanActivateAbility();

        protected abstract void CommitAbility();

        public abstract void ActivateAbility();

        public abstract bool TryActivateAbility();
        public abstract void EndAbility();
    }
}