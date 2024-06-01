using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public interface IHaveHealth
    {
        public int GetCurrentHealth();
        public int GetMaxHealth();
    }
}