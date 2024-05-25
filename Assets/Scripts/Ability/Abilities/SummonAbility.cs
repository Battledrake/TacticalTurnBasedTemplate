using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SummonAbility : Ability
    {
        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private UnitId _unitType;

        [SerializeField] private float _summonDuration;

        private Unit _summonedUnit;

        private bool _isActive = false;

        public override void ActivateAbility()
        {
            CommitAbility();

            _summonedUnit = Instantiate(_unitPrefab, _tacticsGrid.GetWorldPositionFromGridIndex(_targetIndex), Quaternion.identity, this.transform);
            _summonedUnit.InitializeUnit(_unitType);
            _tacticsGrid.AddUnitToTile(_targetIndex, _summonedUnit, true);
            _isActive = true;

            AbilityBehaviorComplete(this);
        }

        public override bool CanActivateAbility()
        {
            if (_tacticsGrid.IsIndexValid(_targetIndex) && _tacticsGrid.IsTileWalkable(_targetIndex) && _tacticsGrid.GridTiles[_targetIndex].unitOnTile == null)
            {
                return true;
            }
            return false;
        }

        public override void EndAbility()
        {
            _isActive = false;
            _summonedUnit.GetComponent<IUnitAnimation>().PlayDeathAnimation();
            _tacticsGrid.RemoveUnitFromTile(_summonedUnit.UnitGridIndex);
            Destroy(_summonedUnit.gameObject, 3f);
            Destroy(this.gameObject, 5f);
        }

        public override bool TryActivateAbility()
        {
            if (CanActivateAbility())
            {
                ActivateAbility();
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