using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SwordThrowAbility : Ability
    {
        public override void ActivateAbility()
        {
        }

        public override bool CanActivateAbility()
        {
            return true;
        }

        public override void EndAbility()
        {
            Destroy(this.gameObject);
        }

        public override bool TryActivateAbility()
        {
            if(CanActivateAbility())
            {
                ActivateAbility();
                return true;
            }
            return false;
        }

        protected override void CommitAbility()
        {
        }
    }
}