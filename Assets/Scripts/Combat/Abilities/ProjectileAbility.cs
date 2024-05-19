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
        private List<Vector3> _targetPositions = new List<Vector3>();
        private List<GameObject> _spawnedObjects = new List<GameObject>();

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
            for(int i = 0; i < _targetIndexes.Count; i++)
            {
                _targetPositions.Add(_tacticsGrid.GetWorldPositionFromGridIndex(_targetIndexes[i]));
                GameObject projectile = Instantiate(_projectilePrefab, _startPosition, Quaternion.identity, this.transform);
                _spawnedObjects.Add(projectile);
            }


            _startMoving = true;
        }

        public override void EndAbility()
        {
            _startMoving = false;
            //Do stuff to target
            //Target.ApplyDamage(10) or something.
            for(int i = 0; i < _spawnedObjects.Count; i++)
            {
                GameObject explosion = Instantiate(_impactPrefab, _spawnedObjects[i].transform.position, Quaternion.identity, this.transform);
                Destroy(_spawnedObjects[i]);
            }

            Destroy(this.gameObject, 2f);
        }

        private void Update()
        {
            if (_startMoving)
            {
                _moveTimer += Time.deltaTime * _moveSpeed;
                float height = _projectileCurve.Evaluate(_moveTimer);

                for(int i = 0; i < _targetPositions.Count; i++)
                {
                    Vector3 lerpPosition = Vector3.Lerp(_startPosition, _targetPositions[i] + new Vector3(0f, height, 0f), _moveTimer);
                    _spawnedObjects[i].transform.position = lerpPosition;

                }

                if (Vector3.Distance(_spawnedObjects[0].transform.position, _targetPositions[0]) < .2)
                {
                    //AbilityTaskCompleted?.Invoke
                    EndAbility();
                }
            }
        }
    }
}