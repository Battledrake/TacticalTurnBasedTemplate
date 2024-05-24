using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MeleeHitAbility : Ability
    {
        [SerializeField] private PlayAnimationTask _playAnimTaskPrefab;

        public override void ActivateAbility()
        {
            CommitAbility();

            if (_instigator)
                _instigator.LookAtTarget(_targetIndex);

            PlayAnimationTask animationTask = Instantiate(_playAnimTaskPrefab);

            animationTask.InitTask(_instigator, "Attack");
            animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
            animationTask.OnTaskCompleted += AbilityTask_OnTaskCompleted;

            StartCoroutine(animationTask.ExecuteTask(this));
        }

        private void AbilityTask_OnTaskCompleted(AbilityTask task)
        {
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
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