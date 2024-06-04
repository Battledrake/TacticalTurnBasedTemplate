using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class RemoveUnitFromGridAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            if (_playerActions.TacticsGrid.GetTileDataFromIndex(index, out TileData tileData))
            {
                if (tileData.unitOnTile != null)
                {
                    CombatManager.Instance.RemoveUnitFromCombat(tileData.unitOnTile, true);

                    return true;
                }
            }
            return false;
        }
    }
}