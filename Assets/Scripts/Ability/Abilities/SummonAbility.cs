using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SummonAbility : FixedAbility
    {
        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private UnitId _unitType;

        [SerializeField] private float _summonDuration;

        private Unit _summonedUnit;

        private bool _isActive = false;

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
            animationTask.InitTask(activationData, _owner.GetComponent<IPlayAnimation>(), AnimationType.Cast, 3f);
            animationTask.OnAnimationEvent += PlayAnimationTask_OnAnimationEvent;
            animationTask.OnTaskCompleted += AbilityTask_OnTaskCompleted;
            animationTask.OnAnimationCancelled += AbilityTask_OnAnimationCancelled;

            StartCoroutine(animationTask.ExecuteTask());
        }        //Something went wrong with the animation. Still apply Effect.
        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask task, AbilityActivationData activationData)
        {
            task.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            task.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;



            _summonedUnit = Instantiate(_unitPrefab, activationData.tacticsGrid.GetWorldPositionFromGridIndex(activationData.targetIndex), Quaternion.identity);
            _summonedUnit.InitUnit(_unitType);
            CombatManager.Instance.AddUnitToCombat(activationData.targetIndex, _summonedUnit, _owner.GetOwningUnit().TeamIndex);
            _isActive = true;

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

            _summonedUnit = Instantiate(_unitPrefab, activationData.tacticsGrid.GetWorldPositionFromGridIndex(activationData.targetIndex), Quaternion.identity);
            _summonedUnit.InitUnit(_unitType);
            CombatManager.Instance.AddUnitToCombat(activationData.targetIndex, _summonedUnit, _owner.GetOwningUnit().TeamIndex);
            _isActive = true;
        }

        public override bool CanActivateAbility(AbilityActivationData activationData)
        {
            if (base.CanActivateAbility(activationData))
            {
                activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData);

                if (activationData.tacticsGrid.IsTileWalkable(activationData.targetIndex) && !targetData.unitOnTile)
                    return true;
            }

            return false;
        }
    }
}