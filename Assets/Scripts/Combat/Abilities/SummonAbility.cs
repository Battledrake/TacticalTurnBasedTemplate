using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SummonAbility : Ability
    {
        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private UnitType _unitType;

        [SerializeField] private float _summonDuration;

        private Unit _summonedUnit;

        private bool _isActive = false;

        public override void ActivateAbility()
        {
        }

        public override bool CanActivateAbility()
        {
            if (_tacticsGrid.IsIndexValid(_aoeIndexes[0]) && _tacticsGrid.IsTileWalkable(_aoeIndexes[0]) && _tacticsGrid.GridTiles[_aoeIndexes[0]].unitOnTile == null)
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
                _summonedUnit = Instantiate(_unitPrefab, _tacticsGrid.GetWorldPositionFromGridIndex(_aoeIndexes[0]), Quaternion.identity, this.transform);
                _summonedUnit.InitializeUnit(_unitType);
                _tacticsGrid.AddUnitToTile(_aoeIndexes[0], _summonedUnit, true);
                _isActive = true;

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