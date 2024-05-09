using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class DecreaseTileHeightAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                if (_playerActions.TacticsGrid.GridTiles.TryGetValue(index, out TileData tileData))
                {
                    Vector3 tilePosition = tileData.tileMatrix.GetPosition();

                    float tileHeight = _playerActions.TacticsGrid.TileSize.z;
                    tilePosition.y -= tileHeight;
                    tileData.tileMatrix = Matrix4x4.TRS(tilePosition, tileData.tileMatrix.rotation, tileData.tileMatrix.lossyScale);

                    _playerActions.TacticsGrid.AddGridTile(tileData);

                    return true;
                }
            }
            return false;
        }
    }
}