using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public interface IHealthVisual
    {
        public int Health { get; }
        public int MaxHealth { get; }
    }
}