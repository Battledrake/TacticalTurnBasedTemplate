using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class AddTileAction : ActionBase
    {
        public override bool ExecuteAction(Vector2Int index)
        {
            if (!_playerActions.TacticsGrid.IsIndexValid(index))
            {
                TileData newTile = new TileData();
                newTile.index = index;
                newTile.tileType = TileType.Normal;

                Vector3 tilePosition = _playerActions.TacticsGrid.GetTilePositionFromGridIndex(index);
                Quaternion tileRotation = _playerActions.TacticsGrid.GetTileRotationFromGridIndex(index);
                Vector3 tileSize = _playerActions.TacticsGrid.TileSize;

                newTile.tileMatrix = Matrix4x4.TRS(tilePosition, tileRotation, tileSize);

                _playerActions.TacticsGrid.AddGridTile(newTile);

                return true;
            }
            return false;
        }
    }
}