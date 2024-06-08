using System;
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
        }

        //Something went wrong with the animation. Still apply Effect.
        private void AbilityTask_OnAnimationCancelled(PlayAnimationTask task, AbilityActivationData activationData)
        {
            task.OnAnimationEvent -= PlayAnimationTask_OnAnimationEvent;
            task.OnTaskCompleted -= AbilityTask_OnTaskCompleted;
            task.OnAnimationCancelled -= AbilityTask_OnAnimationCancelled;

            SummonUnit(activationData);

            EndAbility();
        }

        private void SummonUnit(AbilityActivationData activationData)
        {
            _summonedUnit = Instantiate(_unitPrefab, activationData.tacticsGrid.GetWorldPositionFromGridIndex(activationData.targetIndex), Quaternion.identity);
            _summonedUnit.InitUnit(_unitType);

            CombatManager.Instance.AddUnitToCombat(activationData.targetIndex, _summonedUnit, _owner.GetOwningUnit().TeamIndex);

            _summonedUnit.OnUnitDied += Unit_OnSummonedUnitDied;
            if (_owner.GetOwningUnit())
                _owner.GetOwningUnit().OnUnitDied += Unit_OnOwningUnitDied;

            _isActive = true;
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

            SummonUnit(activationData);
        }

        private void Unit_OnOwningUnitDied(Unit unit, bool shouldDestroy)
        {
            unit.OnUnitDied -= Unit_OnOwningUnitDied;
            _summonedUnit.Die(true);
        }

        private void Unit_OnSummonedUnitDied(Unit unit, bool shouldDestroy)
        {
            _summonedUnit.OnUnitDied -= Unit_OnSummonedUnitDied;

            if (_owner.GetOwningUnit())
                _owner.GetOwningUnit().OnUnitDied -= Unit_OnSummonedUnitDied;

            Destroy(_summonedUnit.gameObject, 2f);
            _isActive = false;
        }

        public override bool CanActivateAbility(AbilityActivationData activationData)
        {
            if (!_summonedUnit && _isActive)
                _isActive = false;

            if (_isActive) return false;

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