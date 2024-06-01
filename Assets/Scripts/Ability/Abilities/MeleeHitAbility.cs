using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MeleeHitAbility : FixedAbility
    {
        [SerializeField] private AnimationType _animationType;

        [SerializeField] private GameObject _impactFxPrefab;

        public override void ActivateAbility(AbilityActivationData activationData)
        {
            CommitAbility();

            if (_instigator)
            {
                _instigator.LookAtTarget(activationData.targetIndex);
            }

            SpawnTaskAndExecute(activationData);
        }

        private void SpawnTaskAndExecute(AbilityActivationData activationData)
        {
            PlayAnimationTask animationTask = new GameObject("PlayAnimationTask", typeof(PlayAnimationTask)).GetComponent<PlayAnimationTask>();

            animationTask.InitTask(activationData, _owner.GetComponent<IPlayAnimation>(), _animationType, 2f);
            animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
            animationTask.OnTaskCompleted += AbilityTask_OnTaskCompleted;
            animationTask.OnAnimationCancelled += AbilityTask_OnAnimationCancelled;

            StartCoroutine(animationTask.ExecuteTask(this));
        }

        //Something went wrong with the animation. Still apply Effect.
        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask task, AbilityActivationData activationData)
        {
            task.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            task.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;

            activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData);
            CombatManager.Instance.ApplyEffectsToTarget(_owner, targetData.unitOnTile.GetComponent<IAbilitySystem>().GetAbilitySystem(), _effects);
            AbilityBehaviorComplete(this);
            ActionCameraController.Instance.HideActionCamera();
            EndAbility();
        }

        private void AbilityTask_OnTaskCompleted(AbilityTask task)
        {
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            AbilityBehaviorComplete(this);
            if (this.Instigator)
                ActionCameraController.Instance.HideActionCamera();
            EndAbility();
        }

        private void PlayAnimationTask_OnAnimationEvent(PlayAnimationTask animationTask, AbilityActivationData activationData)
        {
            animationTask.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData);
            CombatManager.Instance.ApplyEffectsToTarget(_owner, targetData.unitOnTile.GetComponent<IAbilitySystem>().GetAbilitySystem(), _effects);

            GameObject hitFx = Instantiate(_impactFxPrefab, targetData.tileMatrix.GetPosition() + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
            Destroy(hitFx, 2f);
        }

        public override bool CanActivateAbility()
        {
            return true;
        }

        public override bool TryActivateAbility(AbilityActivationData activationData)
        {
            if (CanActivateAbility())
                ActivateAbility(activationData);

            return CanActivateAbility();
        }

        protected override void CommitAbility()
        {
        }
    }
}