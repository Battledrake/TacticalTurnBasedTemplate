using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class MeleeHitAbility : Ability
    {
        [SerializeField] private AnimationType _animationType;

        [SerializeField] private GameObject _impactFxPrefab;

        public override void ActivateAbility()
        {
            CommitAbility();

            if (_instigator)
            {
                _instigator.LookAtTarget(_targetIndex);
            }

            _tacticsGrid.GetTileDataFromIndex(_originIndex, out TileData originData);
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);
            if (originData.unitOnTile)
            {
                if (targetData.unitOnTile)
                    ActionCameraController.Instance.ShowRandomAction(originData.unitOnTile.transform, targetData.unitOnTile.LookAtTransform);
                else
                    ActionCameraController.Instance.ShowRandomAction(originData.unitOnTile.transform, originData.unitOnTile.LookAtTransform);

                ActionCameraController.Instance.OnActionCameraInPosition += ActionCameraInPosition;
            }
            else
            {
                SpawnTaskAndExecute();
            }
        }

        private void SpawnTaskAndExecute()
        {
            PlayAnimationTask animationTask = new GameObject("PlayAnimationTask", typeof(PlayAnimationTask)).GetComponent<PlayAnimationTask>();

            animationTask.InitTask(_instigator, _animationType, 2f);
            animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
            animationTask.OnTaskCompleted += AbilityTask_OnTaskCompleted;
            animationTask.OnAnimationCancelled += AbilityTask_OnAnimationCancelled;

            StartCoroutine(animationTask.ExecuteTask(this));
        }

        private void ActionCameraInPosition()
        {
            ActionCameraController.Instance.OnActionCameraInPosition -= ActionCameraInPosition;

            SpawnTaskAndExecute();
        }

        //Something went wrong with the animation. Still apply Effect.
        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask task)
        {
            task.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            task.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;

            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);
            CombatManager.Instance.ApplyEffectsToUnit(_instigator, targetData.unitOnTile, _effects);
            AbilityBehaviorComplete(this);
            ActionCameraController.Instance.HideActionCamera();
            EndAbility();
        }

        private void AbilityTask_OnTaskCompleted(AbilityTask task)
        {
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            AbilityBehaviorComplete(this);
            ActionCameraController.Instance.HideActionCamera();
            EndAbility();
        }

        private void PlayAnimationTask_OnAnimationEvent(PlayAnimationTask animationTask)
        {
            animationTask.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);
            CombatManager.Instance.ApplyEffectsToUnit(_instigator, targetData.unitOnTile, _effects);

            GameObject hitFx = Instantiate(_impactFxPrefab, targetData.tileMatrix.GetPosition() + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
            Destroy(hitFx, 2f);
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