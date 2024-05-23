using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ProjectileAbility : Ability
    {
        [Header("Projectile Ability")]

        [Header("Projectile Ability Dependencies")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private AnimateObjectTask _taskPrefab;
        [SerializeField] private AnimateObjectTaskData _taskData;

        [Header("Custom Values")]
        [SerializeField] private float _animationTime = 1f;
        [SerializeField] private float _animationSpeed;

        private bool _isActive;
        private float _timeElapsed;
        private Vector3 _startPosition;

        public override bool CanActivateAbility()
        {
            //If owner component allows.
            return true;
        }

        public override bool TryActivateAbility()
        {
            if (!CanActivateAbility())
                return false;

            ActivateAbility();
            return true;
        }
        protected override void CommitAbility()
        {
        }

        public override void ActivateAbility()
        {
            //ExecuteAbilityTask(Action action);
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData initialTargetData);
            _startPosition = initialTargetData.tileMatrix.GetPosition();
            for (int i = 0; i < _aoeIndexes.Count; i++)
            {
                if (_aoeIndexes[i] != _targetIndex)
                {
                    _tacticsGrid.GetTileDataFromIndex(_aoeIndexes[i], out TileData targetData);

                    GameObject projectile = Instantiate(_projectilePrefab);
                    AnimateObjectTask newTask = Instantiate(_taskPrefab);
                    newTask.InitTask(projectile, _taskData, _startPosition, targetData.tileMatrix.GetPosition(), UnityEngine.Random.Range(.8f, _animationSpeed), false);
                    newTask.OnInitialAnimationCompleted += AnimateObjectTask_OnInitialAnimationCompleted;
                    StartCoroutine(newTask.ExecuteTask());
                }
            }


            _isActive = true;
        }

        private void AnimateObjectTask_OnInitialAnimationCompleted(AnimateObjectTask task)
        {

            EndAbility();
        }

        public override void EndAbility()
        {

            AbilityBehaviorComplete(this);

            Destroy(gameObject, 2f);
        }
    }
}