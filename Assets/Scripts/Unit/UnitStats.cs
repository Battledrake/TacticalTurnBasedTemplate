using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AttributeId
    {
        Health,
        MaxHealth,
        MoveRange,
        Agility
    }

    [Serializable]
    public struct AttributeData
    {
        public AttributeId id;
        public int baseValue;
        private int _currentValue;

        public int GetCurrentValue() { return _currentValue; }
        public void SetCurrentValue(int newValue) { _currentValue = newValue; }
    }

    [Serializable]
    public class UnitStats
    {
        public List<TileType> validTileTypes = new List<TileType> { TileType.Normal, TileType.DoubleCost, TileType.TripleCost };
        public bool canMoveDiagonal = true;
        public float heightAllowance = 2f;
        public int maxHealth = 10;
        public int moveRange = 5;
        public int agility = 1;
        public List<Ability> abilities;

        public List<AttributeData> attributes;
    }
}