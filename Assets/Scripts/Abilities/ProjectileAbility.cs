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

        [SerializeField] private float _animationTime = 1f;
        [SerializeField] private float _animationSpeed;

        private bool _isActive;
        private float _timeElapsed;
        private Vector3 _startPosition;
        private List<Vector3> _targetPositions = new List<Vector3>();
        private List<GameObject> _spawnedObjects = new List<GameObject>();
        private List<GameObject> _explosionObjects = new List<GameObject>();

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

                    _targetPositions.Add(targetData.tileMatrix.GetPosition());
                    GameObject projectile = Instantiate(_projectilePrefab, _startPosition, Quaternion.identity, this.transform);
                    projectile.transform.LookAt(targetData.tileMatrix.GetPosition());
                    _spawnedObjects.Add(projectile);
                }
            }


            _isActive = true;
        }

        public override void EndAbility()
        {
            //Do stuff to target
            //Target.ApplyDamage(10) or something.
            for (int i = 0; i < _spawnedObjects.Count; i++)
            {
                //GameObject explosion = Instantiate(_impactPrefab, _spawnedObjects[i].transform.position, Quaternion.identity, this.transform);
                //_explosionObjects.Add(explosion);

                _spawnedObjects[i].SetActive(false);
            }

            AbilityBehaviorComplete(this);

            Destroy(gameObject, 2f);
        }

        private void Update()
        {
            if (_isActive)
            {
                _timeElapsed += Time.deltaTime * _animationSpeed;
                float height = _projectileCurve.Evaluate(_timeElapsed);

                for (int i = 0; i < _targetPositions.Count; i++)
                {
                    Vector3 lerpPosition = Vector3.Lerp(_startPosition, _targetPositions[i] + new Vector3(0f, height, 0f), _timeElapsed);
                    _spawnedObjects[i].transform.position = lerpPosition;

                }

                if (_timeElapsed > _animationTime)
                {
                    _isActive = false;
                    EndAbility();
                }
            }
        }
    }
}