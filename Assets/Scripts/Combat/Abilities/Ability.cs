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
        /// <summary>
        /// Data for determining the range pattern from the source to the target. Includes Pattern enum, MinMax values, and line of sight data.
        /// </summary>
        [SerializeField] protected AbilityRangeData _toTargetData;
        /// <summary>
        /// Data for determining the AOE pattern on the target. Includes Pattern Enum, MinMax values, and line of sight data.
        /// </summary>
        [SerializeField] protected AbilityRangeData _onTargetData;

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