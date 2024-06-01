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

        public override void ActivateAbility(AbilityActivationData activateData)
        {
            CommitAbility();

            _summonedUnit = Instantiate(_unitPrefab, activateData.tacticsGrid.GetWorldPositionFromGridIndex(activateData.targetIndex), Quaternion.identity);
            _summonedUnit.InitUnit(_unitType);
            CombatManager.Instance.AddUnitToCombat(activateData.targetIndex, _summonedUnit);
            _isActive = true;

            AbilityBehaviorComplete(this);
        }

        public override bool CanActivateAbility()
        {
            //if (_tacticsGrid.IsIndexValid(_targetIndex) && _tacticsGrid.IsTileWalkable(_targetIndex) && _tacticsGrid.GridTiles[_targetIndex].unitOnTile == null)
            //{
            //    return true;
            //}
            return true;
        }

        public override void EndAbility()
        {
            _isActive = false;
            if (_summonedUnit != null)
                _summonedUnit.Die(true);

            base.EndAbility();
        }

        public override bool TryActivateAbility(AbilityActivationData activateData)
        {
            if (CanActivateAbility())
            {
                ActivateAbility(activateData);
                return true;
            }
            return false;
        }

        protected override void CommitAbility()
        {
        }

        private void Update()
        {
            if (_isActive)
            {
                _summonDuration -= Time.deltaTime;
                if (_summonDuration <= 0)
                {
                    EndAbility();
                }
            }
        }
    }
}