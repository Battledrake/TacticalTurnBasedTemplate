using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SelectTileAndUnitAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            _playerActions.SetSelectedTileAndUnit(index);
            return true;
        }
    }
}
