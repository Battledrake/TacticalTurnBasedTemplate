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

    public class AbilityData
    {
        private AbilityRangePattern _rangePattern;
        private Vector2Int _rangeMinMax;
    }

    public abstract class Ability : MonoBehaviour
    {
        
        [SerializeField] protected AbilityRangePattern _rangePattern;
        [SerializeField] protected Vector2Int _rangeMinMax;

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