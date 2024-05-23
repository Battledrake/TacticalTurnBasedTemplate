using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AttributeType
    {
        CurrentHealth,
        MaxHealth,
        MoveRange
    }

    [CreateAssetMenu(fileName = "Stats", menuName = "TTBT/Unit/UnitStats")]
    public class UnitStats : ScriptableObject
    {
        public List<TileType> validTileTypes = new List<TileType> { TileType.Normal, TileType.DoubleCost, TileType.TripleCost };
        public bool canMoveDiagonal = true;
        public float heightAllowance = 2f;
        public float moveSpeed = 3f;
        public float moveRange = 5f;
        public int currentHealth = 10;
        public int maxHealth = 10;
        public List<Ability> abilities;
    }
}