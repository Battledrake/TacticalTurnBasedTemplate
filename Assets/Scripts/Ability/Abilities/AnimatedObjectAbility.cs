using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AnimatedObjectAbility : Ability
    {
        [Header("Animated Object Ability")]

        [SerializeField] private GameObject _objectToAnimate;
        [SerializeField] private AnimateObjectTask _taskPrefab;
        [SerializeField] private AnimateObjectTaskData _taskData;

        [SerializeField] private float _animationSpeed = 1f;
        [SerializeField] private bool _loopAnimation = false;

        private AnimateObjectTask _activeTask;
        private GameObject _animatingObject;

        private List<Unit> _hitUnits = new List<Unit>();

        /// <summary>
        /// Set values for update calls.
        /// </summary>
        public override void ActivateAbility()
        {
            CommitAbility();
            _tacticsGrid.GetTileDataFromIndex(_originIndex, out TileData originData);
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);

            AnimateObjectTask task = Instantiate(_taskPrefab);
            if (task)
            {
                _activeTask = task;
                task.OnInitialAnimationCompleted += AnimateObjectTask_OnInitialAnimationComplete;
                task.OnObjectCollisionWithUnit += AnimateObjectTask_OnObjectCollisionWithUnit;

                GameObject objectToAnimate = Instantiate(_objectToAnimate);
                _animatingObject = objectToAnimate;
                task.InitTask(objectToAnimate, _taskData, originData.tileMatrix.GetPosition(), targetData.tileMatrix.GetPosition(), _animationSpeed, _loopAnimation);
                StartCoroutine(task.ExecuteTask());
            }
        }

        private void AnimateObjectTask_OnObjectCollisionWithUnit(Unit unit)
        {
            //TODO: Improve on the friendly fire logic
            if (unit == _instigator && !this.AffectFriendly)
                return;

            if (_hitUnits.Contains(unit))
                return;


            if (unit.UnitGridIndex != _targetIndex && !CombatSystem.Instance.GetAbilityRange(_targetIndex, this.AreaOfEffectData).Contains(unit.UnitGridIndex))
            {
                return;
            }


            _hitUnits.Add(unit);
            CombatSystem.Instance.ApplyEffectsToUnit(_instigator, unit, _effects);
        }

        private void AnimateObjectTask_OnInitialAnimationComplete(AnimateObjectTask task)
        {
            task.OnInitialAnimationCompleted -= AnimateObjectTask_OnInitialAnimationComplete;
            AbilityBehaviorComplete(this);
            if (!_loopAnimation)
            {
                Destroy(_animatingObject);
                EndAbility();
            }
        }

        /// <summary>
        /// Process Cooldown checks. Ensure tiles are valid for this ability.
        /// </summary>
        /// <returns></returns>
        public override bool CanActivateAbility()
        {
            return true;
        }

        /// <summary>
        /// Checks if the ability can be activated before activating
        /// </summary>
        /// <returns></returns>
        public override bool TryActivateAbility()
        {
            if (CanActivateAbility())
            {
                ActivateAbility();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Use AP, Resources, Item, Etc...
        /// </summary>
        protected override void CommitAbility()
        {
        }
    }
}