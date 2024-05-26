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

        [SerializeField] private AnimationType _animationType = AnimationType.Attack;
        [SerializeField] private float _animationSpeed = 1f;
        [SerializeField] private bool _loopAnimation = false;

        private List<Unit> _hitUnits = new List<Unit>();

        /// <summary>
        /// Set values for update calls.
        /// </summary>
        public override void ActivateAbility()
        {
            CommitAbility();

            if (_instigator)
            {
                _instigator.LookAtTarget(_targetIndex);
                _instigator.GetComponent<IUnitAnimation>().PlayAnimationType(_animationType);
            }

            _tacticsGrid.GetTileDataFromIndex(_originIndex, out TileData originData);
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);

            AnimateObjectTask task = Instantiate(_taskPrefab);
            if (task)
            {
                task.OnInitialAnimationCompleted += AnimateObjectTask_OnInitialAnimationComplete;
                task.OnObjectCollisionWithUnit += AnimateObjectTask_OnObjectCollisionWithUnit;

                GameObject objectToAnimate = Instantiate(_objectToAnimate);

                task.InitTask(objectToAnimate, _taskData, originData.tileMatrix.GetPosition(), targetData.tileMatrix.GetPosition(), _animationSpeed, _loopAnimation);
                StartCoroutine(task.ExecuteTask(this));
            }
        }

        private void AnimateObjectTask_OnObjectCollisionWithUnit(Unit unit)
        {
            //TODO: Improve on the friendly fire logic
            if (unit == _instigator && !this.IsFriendly)
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

            //HACK: If somehow our object didn't hit a valid unit due to a collision miss or something? hit it here and possibly fix issue that prevented collision in the first place. If Possible.
            for (int i = 0; i < _aoeIndexes.Count; i++)
            {
                _tacticsGrid.GetTileDataFromIndex(_aoeIndexes[i], out TileData tileData);
                if (tileData.unitOnTile)
                {
                    if (!_hitUnits.Contains(tileData.unitOnTile))
                    {
                        Debug.LogWarning($"Unit at {tileData.index} missed by ability collision. Applying late effect");
                        CombatSystem.Instance.ApplyEffectsToUnit(_instigator, tileData.unitOnTile, _effects);
                    }
                }
            }

            AbilityBehaviorComplete(this);
            if (!_loopAnimation)
            {
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