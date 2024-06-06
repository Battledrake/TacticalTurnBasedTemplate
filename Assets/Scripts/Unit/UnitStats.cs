using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AttributeType
    {
        CurrentHealth,
        MaxHealth,
        MoveRange,
        Agility
    }

    //public struct Attribute
    //{
    //    public AttributeType type;
    //    public int attributeValue;

    //    public Attribute(AttributeType type, int attributeValue)
    //    {
    //        this.type = type;
    //        this.attributeValue = attributeValue;
    //    }
    //}

    //public class AttributeSet
    //{
    //    HashSet<Attribute> _attributes = new HashSet<Attribute>
    //    {
    //        new Attribute(AttributeType.CurrentHealth, 0),
    //        new Attribute(AttributeType.MaxHealth, 0),
    //        new Attribute(AttributeType.MoveRange, 0)
    //    };
    //}

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
    }
}