using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AddTileAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            if (!_playerActions.TacticsGrid.IsIndexValid(index))
            {
                TileData newTile = new TileData();
                newTile.index = index;
                newTile.tileType = TileType.Normal;

                Vector3 tilePosition = _playerActions.TacticsGrid.GetWorldPositionFromGridIndex(index);
                _playerActions.TacticsGrid.TraceForGroundAndObstacles(tilePosition, out Vector3 hitPosition, out Vector3 hitNormal);
                Quaternion tileRotation = _playerActions.TacticsGrid.GetTileRotationFromGridIndex(index);
                tileRotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * tileRotation;
                Vector3 tileSize = _playerActions.TacticsGrid.TileSize;

                newTile.tileMatrix = Matrix4x4.TRS(hitPosition, tileRotation, tileSize);

                _playerActions.TacticsGrid.AddGridTile(newTile);

                return true;
            }
            return false;
        }
    }
}