using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class KillUnitAbility : FixedAbility
    {
        public override void ActivateAbility(AbilityActivationData activationData)
        {
            CommitAbility();

            activationData.tacticsGrid.GetTileDataFromIndex(activationData.targetIndex, out TileData targetData);
            if (targetData.unitOnTile)
            {
                CombatManager.Instance.ApplyAbilityEffectsToTarget(_owner, targetData.unitOnTile.AbilitySystem, this);
            }

            EndAbility();
        }
    }
}