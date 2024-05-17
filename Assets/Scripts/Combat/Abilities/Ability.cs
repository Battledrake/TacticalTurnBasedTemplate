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

    public enum TargetType
    {
        Self,
        Enemy,
        EmptyTile,
        AnyNotSelf,
        Any
    }

    public enum ToTargetPattern
    {
        None,
        MoveTo,
        Projectile,
        Line,
        Cone
    }

    public enum TargetPattern
    {
        Single,
        AOE,
        Chain,
        Relocate
    }

    public class AbilityData
    {
        private AbilityRangePattern _rangePattern;
        private Vector2Int _rangeMinMax;
    }

    public abstract class Ability : MonoBehaviour
    {
        
        [SerializeField] protected AbilityRangePattern _rangePattern;
        [SerializeField] public Vector2Int _rangeMinMax;
        [SerializeField] public TargetType _targetType;
        [SerializeField] public ToTargetPattern _toTargetPattern;
        [SerializeField] public TargetPattern _targetPattern;
        [SerializeField] protected bool _requireLineOfSight;

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