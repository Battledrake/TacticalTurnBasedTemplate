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
        [Tooltip("Delay before animation task cancels itself")]
        [SerializeField] private float _animTaskCancelDelay = 3f;

        public override void ActivateAbility(AbilityActivationData activationData)
        {
            CommitAbility();

            if (_owner.GetOwningUnit())
            {
                _owner.GetOwningUnit().LookAtTarget(activationData.targetIndex);
            }

            SpawnAnimationTaskAndExecute(activationData);
        }

        private void SpawnAnimationTaskAndExecute(AbilityActivationData activationData)
        {
            PlayAnimationTask animationTask = new GameObject("PlayAnimationTask", typeof(PlayAnimationTask)).GetComponent<PlayAnimationTask>();
            animationTask.transform.SetParent(this.transform);
            animationTask.InitTask(activationData, _owner.GetComponent<IPlayAnimation>(), _animationType, _animTaskCancelDelay);
            animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
            animationTask.OnTaskCompleted += AbilityTask_OnTaskCompleted;
            animationTask.OnAnimationCancelled += AbilityTask_OnAnimationCancelled;

            StartCoroutine(animationTask.ExecuteTask());
        }

        //Something went wrong with the animation. Still apply Effect.
        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask task, AbilityActivationData activationData)
        {
            task.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            task.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;

            activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData);
            CombatManager.Instance.ApplyEffectsToTarget(_owner, targetData.unitOnTile.GetComponent<IAbilitySystem>().GetAbilitySystem(), _effects);

            EndAbility();
        }

        private void AbilityTask_OnTaskCompleted(AbilityTask task)
        {
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;

            EndAbility();
        }

        private void PlayAnimationTask_OnAnimationEvent(PlayAnimationTask animationTask, AbilityActivationData activationData)
        {
            animationTask.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            animationTask.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;

            activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData);
            CombatManager.Instance.ApplyEffectsToTarget(_owner, targetData.unitOnTile.GetComponent<IAbilitySystem>().GetAbilitySystem(), _effects);

            GameObject hitFx = Instantiate(_impactFxPrefab, targetData.tileMatrix.GetPosition() + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
            Destroy(hitFx, 2f);
        }
    }
}