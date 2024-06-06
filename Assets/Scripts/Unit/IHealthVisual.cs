using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public interface IHealthVisual
    {
        public int GetHealth();
        public int GetMaxHealth();
    }
}