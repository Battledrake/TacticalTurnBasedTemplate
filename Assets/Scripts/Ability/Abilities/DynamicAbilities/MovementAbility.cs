using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    /// <summary>
    /// This is not a functioning ability. It's original purpose was to merge movement into the ability system, but that design required a lot of tweaks and checks. A new range pattern for movement and custom logic in the ability use action.
    /// Easier to keep movement separate. This is kept in the event that movement ability reimplementation is desired.
    /// </summary>
    public class MovementAbility : Ability
    {
        private int _moveCost;

        public override AbilityRangeData GetRangeData()
        {
            AbilityRangeData moveRangeData = new AbilityRangeData();
            moveRangeData.rangePattern = AbilityRangePattern.Movement;
            moveRangeData.rangeMinMax = new Vector2Int(1, _owner.GetAttributeCurrentValue(AttributeId.MoveRange));
            return moveRangeData;
        }

        public override AbilityRangeData GetAreaOfEffectData()
        {
            AbilityRangeData sprintRangeData = new AbilityRangeData();
            sprintRangeData.rangePattern = AbilityRangePattern.Movement;
            sprintRangeData.rangeMinMax = new Vector2Int(0, _owner.GetAttributeCurrentValue(AttributeId.MoveRange) * 2);
            return sprintRangeData;
        }

        public override List<RangedAbilityEffect> GetEffects()
        {
            return new List<RangedAbilityEffect>();
        }

        protected override void CommitAbility()
        {
            AbilityEffect moveCostEffect =  _costEffect.effect;
            moveCostEffect.magnitude = _moveCost;
            _owner.ApplyEffect(moveCostEffect); 
        }


        //Do Logic Here
        public override void ActivateAbility(AbilityActivationData activationData)
        {
            _moveCost = CombatManager.Instance.GetAbilityRange(activationData.originIndex, this.GetRangeData()).Contains(activationData.targetIndex) ? 1 : 2;

            CommitAbility();

            //TODO: Move this to a task. MoveToLocationTask?;
            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_owner.GetOwningUnit());

            PathfindingResult pathResult = activationData.tacticsGrid.GridPathfinder.FindPath(activationData.originIndex, activationData.targetIndex, pathParams);
            if (pathResult.Result == PathResult.SearchSuccess)
            {
                CombatManager.Instance.MoveUnit(_owner.GetOwningUnit(), pathResult.Path, pathResult.Length);

                _owner.GetOwningUnit().OnUnitMovementStopped += Instigator_OnUnitMovementStopped;
                _owner.GetOwningUnit().OnUnitReachedDestination += Instigator_OnUnitReachedDestination;
            }
        }

        private void Instigator_OnUnitMovementStopped(Unit unit)
        {
            _owner.GetOwningUnit().OnUnitMovementStopped -= Instigator_OnUnitMovementStopped;
            _owner.GetOwningUnit().OnUnitReachedDestination -= Instigator_OnUnitReachedDestination;

            EndAbility();
        }

        private void Instigator_OnUnitReachedDestination(Unit unit)
        {
            _owner.GetOwningUnit().OnUnitMovementStopped -= Instigator_OnUnitMovementStopped;
            _owner.GetOwningUnit().OnUnitReachedDestination -= Instigator_OnUnitReachedDestination;

            EndAbility();
        }

        //Check for whatever conditions here.
        public override bool CanActivateAbility(AbilityActivationData activationData)
        {
            if (_owner.GetAttributeCurrentValue(AttributeId.ActionPoints) <= 0)
                return false;

            if (_owner.GetAttributeCurrentValue(AttributeId.ActionPoints) == 1 && !CombatManager.Instance.GetAbilityRange(activationData.originIndex, this.GetRangeData()).Contains(activationData.targetIndex))
                return false;

            if (CombatManager.Instance.GetAbilityRange(activationData.originIndex, this.GetAreaOfEffectData()).Contains(activationData.targetIndex))
            {
                GridMovement gridMovement = _owner.GetComponent<GridMovement>();
                if (gridMovement == null) return false;

                if (gridMovement.IsMoving) return false;

                return true;
            }
            return false;
        }

        public override int GetUsesLeft()
        {
            return -1;
        }

        public override void ReduceUsesLeft(int amount)
        {
            //No use in this ability
        }
    }
}