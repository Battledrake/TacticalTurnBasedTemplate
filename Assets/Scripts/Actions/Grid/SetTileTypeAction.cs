using System;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class SetTileTypeAction : ActionBase
    {
        public override bool ExecuteAction(Vector2Int index)
        {
            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                if (_playerActions.TacticsGrid.GridTiles.TryGetValue(index, out TileData tileData))
                {
                    if (Enum.IsDefined(typeof(TileType), actionValue))
                    {
                        tileData.tileType = (TileType)actionValue;

                        _playerActions.TacticsGrid.AddGridTile(tileData);

                        return true;
                    }
                }
            }
            return false;
        }
    }
}