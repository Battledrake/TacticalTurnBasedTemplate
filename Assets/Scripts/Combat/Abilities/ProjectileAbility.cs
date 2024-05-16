using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ProjectileAbility : Ability
    {
        [SerializeField] private AnimationCurve _projectileCurve;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _impactPrefab;

        [SerializeField] private float _moveSpeed;
        private bool _startMoving;
        private float _moveTimer;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private GameObject _spawnedObject;

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
            throw new System.NotImplementedException();
        }

        public override void ActivateAbility()
        {
            //ExecuteAbilityTask(Action action);
            _startPosition = _tacticsGrid.GetWorldPositionFromGridIndex(_originIndex) + Vector3.up;
            _targetPosition = _tacticsGrid.GetWorldPositionFromGridIndex(_targetIndex);
            GameObject projectile = Instantiate(_projectilePrefab, _startPosition, Quaternion.identity, this.transform);
            _spawnedObject = projectile;

            _startMoving = true;
        }

        public override void EndAbility()
        {
            _startMoving = false;
            //Do stuff to target
            //Target.ApplyDamage(10) or something.

            GameObject explosion = Instantiate(_impactPrefab, _spawnedObject.transform.position, Quaternion.identity, this.transform);
            Destroy(_spawnedObject);

            Destroy(this.gameObject, 2f);
        }

        private void Update()
        {
            if (_startMoving)
            {
                _moveTimer += Time.deltaTime * _moveSpeed;
                float height = _projectileCurve.Evaluate(_moveTimer);

                Vector3 lerpPosition = Vector3.Lerp(_startPosition, _targetPosition + new Vector3(0f, height, 0f), _moveTimer);
                _spawnedObject.transform.position = lerpPosition;

                if(Vector3.Distance(_spawnedObject.transform.position, _targetPosition) < .2)
                {
                    //AbilityTaskCompleted?.Invoke
                    EndAbility();
                }
            }
        }
    }
}