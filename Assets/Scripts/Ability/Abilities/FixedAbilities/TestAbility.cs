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

        public override void ActivateAbility(AbilityActivationData activationData)
        {
            _startPosition = activationData.tacticsGrid.GetWorldPositionFromGridIndex(activationData.originIndex) + Vector3.up;
            _targetPosition = activationData.tacticsGrid.GetWorldPositionFromGridIndex(activationData.targetIndex);
            Vector3 lookDirection = _targetPosition - _startPosition;
            GameObject projectile = Instantiate(_vinePrefab, _startPosition, Quaternion.LookRotation(lookDirection), this.transform);
            _spawnedObject = projectile;

            ParticleSystem particleSystem = _spawnedObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule sysMain = particleSystem.main;
            sysMain.startSpeed = Vector3.Distance(_targetPosition, _startPosition) / 2 * 10;
            sysMain.simulationSpeed = _playbackSpeed;

            Invoke("EndAbility", 4f);

            if(activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData))
            {
                if (targetData.unitOnTile)
                {
                    AbilitySystem receiver = targetData.unitOnTile.GetComponent<IAbilitySystem>().GetAbilitySystem();
                    if (receiver)
                    {
                        CombatManager.Instance.ApplyEffectsToTarget(_owner, receiver, _effects);
                    }
                }
            }
        }

        public override void EndAbility()
        {
            Destroy(_spawnedObject);
            base.EndAbility();
        }
    }
}