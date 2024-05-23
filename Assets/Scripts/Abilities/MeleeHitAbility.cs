using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MeleeHitAbility : Ability
    {
        [SerializeField] private float _delay;
        

        public override void ActivateAbility()
        {
            EndAbility();
        }

        public override bool CanActivateAbility()
        {
            return true;
        }

        public override bool TryActivateAbility()
        {
            if (CanActivateAbility())
                ActivateAbility();

            return CanActivateAbility();
        }

        protected override void CommitAbility()
        {
        }
    }
}