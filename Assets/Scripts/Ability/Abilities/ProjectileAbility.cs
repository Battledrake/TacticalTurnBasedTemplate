using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ProjectileAbility : FixedAbility
    {
        [Header("Projectile Ability")]

        [Header("Projectile Ability Dependencies")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private AnimateObjectTaskData _taskData;

        [Header("Custom Values")]
        [SerializeField] private AnimationType _animationType;
        [SerializeField] private float _animationTime = 1f;
        [SerializeField] private float _animationSpeed;

        public override void ActivateAbility(AbilityActivationData activationData)
        {
            CommitAbility();

            Unit unit = _owner.GetOwningUnit();
            if (unit)
            {
                unit.LookAtTarget(activationData.targetIndex);
                unit.GetComponent<IPlayAnimation>().PlayAnimationType(_animationType);
            }


            activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData initialTargetData);
            Vector3 startPosition = initialTargetData.tileMatrix.GetPosition();

            List<GridIndex> aoeIndexes = CombatManager.Instance.GetAbilityRange(activationData.targetIndex, this.GetAreaOfEffectData());

            for (int i = 0; i < aoeIndexes.Count; i++)
            {
                if (aoeIndexes[i] != activationData.targetIndex)
                {
                    activationData.tacticsGrid.GetTileDataFromIndex(aoeIndexes[i], out TileData targetData);

                    GameObject projectile = Instantiate(_projectilePrefab);

                    AnimateObjectTask animateObjectTask = new GameObject("AnimateObjectTask", new[] { typeof(AnimateObjectTask), typeof(Rigidbody) }).GetComponent<AnimateObjectTask>();
                    animateObjectTask.transform.SetParent(this.transform);
                    animateObjectTask.GetComponent<Rigidbody>().useGravity = false;

                    AbilityActivationData modifiedData = activationData;
                    modifiedData.targetIndex = aoeIndexes[i];

                    animateObjectTask.InitTask(projectile, _taskData, modifiedData, _animationTime, UnityEngine.Random.Range(.8f, _animationSpeed), false);

                    animateObjectTask.OnInitialAnimationCompleted += AnimateObjectTask_OnInitialAnimationCompleted;
                    animateObjectTask.OnObjectCollisionWithUnit += AnimateObjectTask_OnObjectCollisionWithUnit;

                    StartCoroutine(animateObjectTask.ExecuteTask());
                }
            }
        }

        private void AnimateObjectTask_OnObjectCollisionWithUnit(AbilitySystem receiver, AbilityActivationData activateData)
        {
            //TODO: Improve on the friendly fire logic
            if (receiver == _owner && !this.IsFriendly)
                return;


            List<GridIndex> aoeIndexes = CombatManager.Instance.GetAbilityRange(activateData.targetIndex, this.GetAreaOfEffectData());

            if (!aoeIndexes.Contains(receiver.GetOwningUnit().UnitGridIndex))
            {
                return;
            }

            CombatManager.Instance.ApplyEffectsToTarget(_owner, receiver, _effects);
        }

        private void AnimateObjectTask_OnInitialAnimationCompleted(AnimateObjectTask animateObjectTask, AbilityActivationData activateData)
        {
            animateObjectTask.OnInitialAnimationCompleted -= AnimateObjectTask_OnInitialAnimationCompleted;
            animateObjectTask.OnObjectCollisionWithUnit -= AnimateObjectTask_OnObjectCollisionWithUnit;

            List<GridIndex> aoeIndexes = CombatManager.Instance.GetAbilityRange(activateData.targetIndex, this.GetAreaOfEffectData());
            //HACK: If somehow our object didn't hit a valid unit due to a collision miss or something? hit it here and possibly fix issue that prevented collision in the first place. If Possible.
            for (int i = 0; i < aoeIndexes.Count; i++)
            {
                activateData.tacticsGrid.GetTileDataFromIndex(aoeIndexes[i], out TileData tileData);
                if (tileData.unitOnTile)
                {
                    AbilitySystem receiver = tileData.unitOnTile.GetComponent<IAbilitySystem>().GetAbilitySystem();
                    if (!animateObjectTask.HitUnits.Contains(receiver))
                    {
                        CombatManager.Instance.ApplyEffectsToTarget(_owner, receiver, _effects);
                    }
                }
            }

            EndAbility();
        }
    }
}