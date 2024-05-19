using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TestAbility : Ability
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
            _targetPosition = _tacticsGrid.GetWorldPositionFromGridIndex(_targetIndexes[0]);
            Vector3 lookDirection = _targetPosition - _startPosition;
            GameObject projectile = Instantiate(_vinePrefab, _startPosition, Quaternion.LookRotation(lookDirection), this.transform);
            _spawnedObject = projectile;

            ParticleSystem sysMain = _spawnedObject.GetComponent<ParticleSystem>();
            sysMain.startSpeed = Vector3.Distance(_targetPosition, _startPosition) / 2 * 10;
            sysMain.playbackSpeed = _playbackSpeed;

            Invoke("EndAbility", 2f);
        }

        public override void EndAbility()
        {
            Destroy(_spawnedObject);

            Destroy(this.gameObject);
        }

        private void Update()
        {
        }
    }
}