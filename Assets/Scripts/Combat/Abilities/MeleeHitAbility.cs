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

        }

        public override bool CanActivateAbility()
        {
            return false;
        }

        public override void EndAbility()
        {
            Destroy(this.gameObject);
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