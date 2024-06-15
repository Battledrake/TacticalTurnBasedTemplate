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
                CombatManager.Instance.ApplyEffectsToTarget(_owner, targetData.unitOnTile.GetComponent<IAbilitySystem>().AbilitySystem, _effects);
            }

            EndAbility();
        }
    }
}