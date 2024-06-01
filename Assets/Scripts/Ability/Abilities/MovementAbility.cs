using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MovementAbility : DynamicAbility
    {
        //Do Logic Here
        public override void ActivateAbility(AbilityActivationData activateData)
        {
            CommitAbility();

            //TODO: Move this to a task. MoveToLocationTask?;
            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_instigator);

            PathfindingResult pathResult = activateData.tacticsGrid.GridPathfinder.FindPath(_instigator.UnitGridIndex, activateData.targetIndex, pathParams);
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
            EndAbility();
        }

        //Check for whatever conditions here.
        public override bool CanActivateAbility(AbilityActivationData activateData)
        {
            if (base.CanActivateAbility(activateData))
            {
                GridMovement gridMovement = _owner.GetComponent<GridMovement>();
                if (gridMovement == null) return false;

                if (gridMovement.IsMoving) return false;
            }

            return true;
        }
    }
}