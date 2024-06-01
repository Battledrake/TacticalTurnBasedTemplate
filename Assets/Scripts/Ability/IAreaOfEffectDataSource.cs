using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public interface IAreaOfEffectDataSource
    {
        public AbilityRangeData GetAreaOfEffectData();
    }
}