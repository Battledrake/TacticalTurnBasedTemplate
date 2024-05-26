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

        private List<Unit> _hitUnits = new List<Unit>();

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
            CommitAbility();

            if (_instigator)
            {
                _instigator.LookAtTarget(_targetIndex);
                _instigator.GetComponent<IUnitAnimation>().PlayAttackAnimation();
            }


            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData initialTargetData);
            Vector3 startPosition = initialTargetData.tileMatrix.GetPosition();
            for (int i = 0; i < _aoeIndexes.Count; i++)
            {
                if (_aoeIndexes[i] != _targetIndex)
                {
                    _tacticsGrid.GetTileDataFromIndex(_aoeIndexes[i], out TileData targetData);

                    GameObject projectile = Instantiate(_projectilePrefab);

                    AnimateObjectTask newTask = Instantiate(_taskPrefab);
                    newTask.InitTask(projectile, _taskData, startPosition, targetData.tileMatrix.GetPosition(), UnityEngine.Random.Range(.8f, _animationSpeed), false);

                    newTask.OnInitialAnimationCompleted += AnimateObjectTask_OnInitialAnimationCompleted;
                    newTask.OnObjectCollisionWithUnit += AnimateObjectTask_OnObjectCollisionWithUnit;

                    StartCoroutine(newTask.ExecuteTask(this));
                }
            }
        }

        private void AnimateObjectTask_OnObjectCollisionWithUnit(Unit unit)
        {
            //TODO: Improve on the friendly fire logic
            if (unit == _instigator && !this.IsFriendly)
                return;

            if (_hitUnits.Contains(unit))
                return;

            if (!_aoeIndexes.Contains(unit.UnitGridIndex))
            {
                return;
            }

            _hitUnits.Add(unit);
            CombatSystem.Instance.ApplyEffectsToUnit(_instigator, unit, _effects);
        }

        private void AnimateObjectTask_OnInitialAnimationCompleted(AnimateObjectTask task)
        {
            task.OnInitialAnimationCompleted -= AnimateObjectTask_OnInitialAnimationCompleted;
            task.OnObjectCollisionWithUnit -= AnimateObjectTask_OnObjectCollisionWithUnit;

            //HACK: If somehow our object didn't hit a valid unit due to a collision miss or something? hit it here and possibly fix issue that prevented collision in the first place. If Possible.
            for(int i = 0; i < _aoeIndexes.Count; i++)
            {
                _tacticsGrid.GetTileDataFromIndex(_aoeIndexes[i], out TileData tileData);
                if (tileData.unitOnTile)
                {
                    if (!_hitUnits.Contains(tileData.unitOnTile))
                    {
                        CombatSystem.Instance.ApplyEffectsToUnit(_instigator, tileData.unitOnTile, _effects);
                    }
                }
            }

            AbilityBehaviorComplete(this);
            EndAbility();
        }
    }
}