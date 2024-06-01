using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TestAbility : FixedAbility
    {
        [SerializeField] private GameObject _vinePrefab;
        [SerializeField] private float _playbackSpeed = 0.25f;

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
            Vector3 lookDirection = _targetPosition - _startPosition;
            GameObject projectile = Instantiate(_vinePrefab, _startPosition, Quaternion.LookRotation(lookDirection), this.transform);
            _spawnedObject = projectile;

            ParticleSystem particleSystem = _spawnedObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule sysMain = particleSystem.main;
            sysMain.startSpeed = Vector3.Distance(_targetPosition, _startPosition) / 2 * 10;
            sysMain.simulationSpeed = _playbackSpeed;

            Invoke("EndAbility", 2f);
        }

        public override void EndAbility()
        {
            AbilityBehaviorComplete(this);

            Destroy(_spawnedObject);

            Destroy(this.gameObject);
        }

        private void Update()
        {
        }
    }
}