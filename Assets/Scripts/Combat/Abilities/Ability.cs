using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class Ability : MonoBehaviour
    {
        
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