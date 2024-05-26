using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MeleeHitAbility : Ability
    {
        [SerializeField] private PlayAnimationTask _playAnimTaskPrefab;
        [SerializeField] private AnimationType _animationType;

        public override void ActivateAbility()
        {
            CommitAbility();

            if (_instigator)
            {
                _instigator.LookAtTarget(_targetIndex);
                _instigator.GetComponent<IUnitAnimation>().PlayAttackAnimation();
            }


            PlayAnimationTask animationTask = Instantiate(_playAnimTaskPrefab);

            animationTask.InitTask(_instigator, _animationType, 2f);
            animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
            animationTask.OnTaskCompleted += AbilityTask_OnTaskCompleted;
            animationTask.OnAnimationCancelled += AbilityTask_OnAnimationCancelled;

            StartCoroutine(animationTask.ExecuteTask(this));
        }

        //Something went wrong with the animation. Still apply Effect.
        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask task)
        {
            task.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            task.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;

            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);
            CombatSystem.Instance.ApplyEffectsToUnit(_instigator, targetData.unitOnTile, _effects);
            AbilityBehaviorComplete(this);
            EndAbility();
        }

        private void AbilityTask_OnTaskCompleted(AbilityTask task)
        {
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            AbilityBehaviorComplete(this);
            EndAbility();
        }

        private void PlayAnimationTask_OnAnimationEvent(PlayAnimationTask animationTask)
        {
            animationTask.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);
            CombatSystem.Instance.ApplyEffectsToUnit(_instigator, targetData.unitOnTile, _effects);
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