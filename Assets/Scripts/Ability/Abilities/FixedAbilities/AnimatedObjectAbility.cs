using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AnimatedObjectAbility : FixedAbility
    {
        [Header("Animated Object Ability")]

        [SerializeField] private GameObject _objectToAnimate;
        [SerializeField] private AnimateObjectTaskData _taskData;

        [SerializeField] private AnimationType _animationType = AnimationType.RangedAttack;
        [Tooltip("How long the animation task runs for.")]
        [SerializeField] private float _animationTime = 1f;
        [Tooltip("How fast the animation plays. 1 is default curve speed")]
        [SerializeField] private float _animationSpeed = 1f;
        [Tooltip("Idea was to allow ability to run after animation, but will likely move to effects. No use currently.")]
        [SerializeField] private bool _loopAnimation = false;

        /// <summary>
        /// Set values for update calls.
        /// </summary>
        public override void ActivateAbility(AbilityActivationData activationData)
        {
            CommitAbility();

            if (_owner.OwningUnit)
            {
                _owner.OwningUnit.LookAtTarget(activationData.targetIndex);

                PlayAnimationTask animationTask = new GameObject("PlayAnimationTask", typeof(PlayAnimationTask)).GetComponent<PlayAnimationTask>();
                animationTask.transform.SetParent(this.transform);

                animationTask.InitTask(activationData, _owner.GetComponent<IPlayAnimation>(), _animationType, 2f);
                animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
                animationTask.OnAnimationCancelled += AbilityTask_OnAnimationCancelled;

                StartCoroutine(animationTask.ExecuteTask());
            }
            else
            {
                SpawnAnimateObjectTaskAndExecute(activationData);
            }

        }

        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask animationTask, AbilityActivationData activationData)
        {
            animationTask.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            animationTask.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;
            SpawnAnimateObjectTaskAndExecute(activationData);
        }

        private void PlayAnimationTask_OnAnimationEvent(PlayAnimationTask animationTask, AbilityActivationData activationData)
        {
            animationTask.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;
            animationTask.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            SpawnAnimateObjectTaskAndExecute(activationData);
        }

        private void SpawnAnimateObjectTaskAndExecute(AbilityActivationData activationData)
        {
            AnimateObjectTask animateObjectTask = new GameObject("AnimateObjectTask", new[] { typeof(AnimateObjectTask), typeof(Rigidbody) }).GetComponent<AnimateObjectTask>();
            animateObjectTask.GetComponent<Rigidbody>().useGravity = false;
            animateObjectTask.transform.SetParent(this.transform);
            if (animateObjectTask)
            {
                animateObjectTask.OnInitialAnimationCompleted += AnimateObjectTask_OnInitialAnimationComplete;
                animateObjectTask.OnObjectCollisionWithUnit += AnimateObjectTask_OnObjectCollisionWithUnit;

                GameObject objectToAnimate = Instantiate(_objectToAnimate);

                animateObjectTask.InitTask(objectToAnimate, _taskData, activationData, _animationTime, _animationSpeed, _loopAnimation);
                StartCoroutine(animateObjectTask.ExecuteTask());
            }
        }

        private void AnimateObjectTask_OnObjectCollisionWithUnit(AbilitySystem receiver, AbilityActivationData activationData)
        {
            if (receiver == _owner && !_isFriendlyOnly) return;
            int instigatorTeamIndex = _owner.OwningUnit ? _owner.OwningUnit.TeamIndex : 8;
            if (instigatorTeamIndex == receiver.OwningUnit.TeamIndex && !_isFriendlyOnly) return;
            if (_isFriendlyOnly && instigatorTeamIndex != receiver.OwningUnit.TeamIndex) return;

            if (receiver.OwningUnit.GridIndex != activationData.targetIndex && !CombatManager.Instance.GetAbilityRange(activationData.targetIndex, this.AreaOfEffectData).Contains(receiver.OwningUnit.GridIndex))
            {
                return;
            }

            CombatManager.Instance.ApplyAbilityEffectsToTarget(_owner, receiver, this);
        }

        private void AnimateObjectTask_OnInitialAnimationComplete(AnimateObjectTask task, AbilityActivationData activationData)
        {
            task.OnObjectCollisionWithUnit -= AnimateObjectTask_OnObjectCollisionWithUnit;
            task.OnInitialAnimationCompleted -= AnimateObjectTask_OnInitialAnimationComplete;

            List<GridIndex> aoeIndexes = CombatManager.Instance.GetAbilityRange(activationData.targetIndex, AreaOfEffectData);

            //HACK: If somehow our object didn't hit a valid unit due to a collision miss or something? hit it here and possibly fix issue that prevented collision in the first place. If Possible.
            for (int i = 0; i < aoeIndexes.Count; i++)
            {
                activationData.tacticsGrid.GetTileDataFromIndex(aoeIndexes[i], out TileData tileData);
                if (tileData.unitOnTile)
                {
                    AbilitySystem receiver = tileData.unitOnTile.AbilitySystem;
                    if (!task.HitUnits.Contains(receiver))
                    {
                        Debug.LogWarning($"Unit at {tileData.index} missed by ability collision. Applying late effect");
                        CombatManager.Instance.ApplyAbilityEffectsToTarget(_owner, receiver, this);
                    }
                }
            }

            EndAbility();
        }
    }
}