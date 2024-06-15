using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class DynamicAbilityExample : Ability
    {
        public override AbilityRangeData RangeData
        {
            get
            {
                AbilityRangeData randomRangeData = new AbilityRangeData();
                AbilityRangePattern[] rangePatterns = Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().ToArray();
                int randomNumber = UnityEngine.Random.Range(0, rangePatterns.Length);
                randomRangeData.rangePattern = rangePatterns[randomNumber];

                randomRangeData.rangeMinMax = new Vector2Int(0, 10);

                return randomRangeData;
            }
        }

        public override AbilityRangeData AreaOfEffectData
        {
            get
            {
                AbilityRangeData randomRangeData = new AbilityRangeData();
                AbilityRangePattern[] rangePatterns = Enum.GetValues(typeof(AbilityRangePattern)).Cast<AbilityRangePattern>().ToArray();
                int randomNumber = UnityEngine.Random.Range(0, rangePatterns.Length);
                randomRangeData.rangePattern = rangePatterns[randomNumber];

                randomRangeData.rangeMinMax = new Vector2Int(0, 10);

                return randomRangeData;
            }
        }

        public override List<RangedAbilityEffect> Effects
        {
            get
            {
                RangedAbilityEffect randomEffect;
                AttributeId[] attributes = Enum.GetValues(typeof(AttributeId)).Cast<AttributeId>().ToArray();
                int randomNumber = UnityEngine.Random.Range(0, attributes.Length);
                randomEffect.durationData = new EffectDurationData() { durationPolicy = EffectDurationPolicy.Infinite };
                randomEffect.attribute = attributes[randomNumber];
                randomEffect.magnitudeRange = new Vector2Int(1, 11);

                return new List<RangedAbilityEffect> { randomEffect };
            }
        }

        public override void ActivateAbility(AbilityActivationData activationData)
        {
            CommitAbility();

            activationData.tacticsGrid.GetTileDataFromIndex(activationData.originIndex, out TileData originData);
            activationData.tacticsGrid.GetTileDataFromIndex(activationData.originIndex, out TileData targetData);

            AbilitySystem receiver;
            if (originData.unitOnTile && targetData.unitOnTile)
            {
                int random = UnityEngine.Random.Range(0, 2);
                receiver = random == 0 ? originData.unitOnTile.GetComponent<IAbilitySystem>().AbilitySystem : targetData.unitOnTile.GetComponent<IAbilitySystem>().AbilitySystem;
            }
            else if (originData.unitOnTile)
            {
                receiver = originData.unitOnTile.GetComponent<IAbilitySystem>().AbilitySystem;
            }
            else if(targetData.unitOnTile)
            {
                receiver = targetData.unitOnTile.GetComponent<IAbilitySystem>().AbilitySystem;
            }
            else
            {
                receiver = _owner;
            }

            CombatManager.Instance.ApplyEffectsToTarget(_owner, receiver, Effects);

            EndAbility();
        }

        public override int UsesLeft => -1;

        public override void ReduceUsesLeft(int amount)
        {
            //Here we could subtract items from an inventory or whatever.
        }
    }
}