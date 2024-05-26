using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MovementAbility : Ability
    {
        //Do Logic Here
        public override void ActivateAbility()
        {
            CommitAbility();

            //TODO: Move this to a task. MoveToLocationTask?;
            PathFilter pathFilter = GridPathfinding.CreatePathFilterFromUnit(_instigator, false, false);

            PathfindingResult pathResult = _tacticsGrid.GridPathfinder.FindPath(_instigator.UnitGridIndex, _targetIndex, pathFilter);
            if (pathResult.Result == PathResult.SearchSuccess)
            {
                _instigator.GetComponent<GridMovement>().SetPathAndMove(pathResult.Path);

                _instigator.OnUnitMovementStopped += Instigator_OnUnitMovementStopped;
                _instigator.OnUnitReachedDestination += Instigator_OnUnitReachedDestination;
            }
        }

        private void Instigator_OnUnitMovementStopped(Unit unit)
        {
            _instigator.OnUnitMovementStopped -= Instigator_OnUnitMovementStopped;
            _instigator.OnUnitReachedDestination -= Instigator_OnUnitReachedDestination;
            //AbilityBehaviorComplete? We'll see when we get the turn and combat system going.
            EndAbility();
        }

        private void Instigator_OnUnitReachedDestination(Unit unit)
        {
            _instigator.OnUnitMovementStopped -= Instigator_OnUnitMovementStopped;
            _instigator.OnUnitReachedDestination -= Instigator_OnUnitReachedDestination;
            AbilityBehaviorComplete(this);
            EndAbility();
        }

        //Check for whatever conditions here.
        public override bool CanActivateAbility()
        {
            if (_instigator == null) return false;

            if (_instigator.IsMoving) return false;

            return true;
        }

        public override bool TryActivateAbility()
        {
            if (CanActivateAbility())
            {
                ActivateAbility();
                return true;
            }
            return false;
        }

        //Ap Cost and stuff here
        protected override void CommitAbility()
        {
        }
    }
}