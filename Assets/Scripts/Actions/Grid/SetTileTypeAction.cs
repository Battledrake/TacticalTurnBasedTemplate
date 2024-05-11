using System;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class SetTileTypeAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                if (_playerActions.TacticsGrid.GridTiles.TryGetValue(index, out TileData tileData))
                {
                    if (Enum.IsDefined(typeof(TileType), actionValue))
                    {
                        tileData.tileType = (TileType)actionValue;

                        if (tileData.tileType == TileType.None)
                            _playerActions.TacticsGrid.RemoveGridTile(tileData.index);
                        else
                            _playerActions.TacticsGrid.AddGridTile(tileData);

                        return true;
                    }
                }
            }
            return false;
        }
    }
}