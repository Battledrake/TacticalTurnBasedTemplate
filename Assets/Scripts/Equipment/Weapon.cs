using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public Sprite icon;
    public AnimationType animationType;

    [Header("RangeData")]
    public AbilityRangeData rangeData;
    public AbilityRangeData areaOfEffectData;

    [Header("Effects")]
    public List<RangedGameplayEffect> targetEffects;
    public List<GameplayEffect> unitEffects;
}
