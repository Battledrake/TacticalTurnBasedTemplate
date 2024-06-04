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

            _summonedUnit = Instantiate(_unitPrefab, activationData.tacticsGrid.GetWorldPositionFromGridIndex(activationData.targetIndex), Quaternion.identity);
            _summonedUnit.InitUnit(_unitType);
            CombatManager.Instance.AddUnitToCombat(activationData.targetIndex, _summonedUnit, _owner.GetOwningUnit().TeamIndex);
            _isActive = true;

            EndAbility();
        }

        public override bool CanActivateAbility(AbilityActivationData activationData)
        {
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