using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AttributeId
    {
        ActionPoints,
        Health,
        MaxHealth,
        MoveRange,
        Agility
    }

    [Serializable]
    public struct AttributeData
    {
        public AttributeId attribute;
        public int baseValue;
        /// <summary>
        /// We keep the currentValue private as its value is always set based on the base value + effect magnitudes.
        /// </summary>
        private int _currentValue;

        public int GetCurrentValue() { return _currentValue; }
        public void SetCurrentValue(int newValue) { _currentValue = newValue; }
    }

    [Serializable]
    public class UnitStats
    {
        public bool canMoveDiagonal = true;
        public float heightAllowance = 2f;
        public List<TileType> validTileTypes = new List<TileType> { TileType.Normal, TileType.DoubleCost, TileType.TripleCost };
        public List<AttributeData> attributes;
        public List<AbilityId> abilities;
    }
}