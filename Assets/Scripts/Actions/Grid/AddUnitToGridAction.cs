using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AddUnitToGridAction : ActionBase
    {
        [SerializeField] private Unit _unitPrefab;
        public override bool ExecuteAction(GridIndex index)
        {
            if (actionValue < 0)
                return false;

            if (_playerActions.TacticsGrid.GetTileDataFromIndex(index, out TileData tileData))
            {
                if (GridStatics.IsTileTypeWalkable(tileData.tileType))
                {
                    if (tileData.unitOnTile == null)
                    {
                        Unit newUnit = Instantiate(_unitPrefab, _playerActions.TacticsGrid.GetTilePositionFromGridIndex(index), Quaternion.identity);
                        newUnit.InitializeUnit((UnitType)actionValue);
                        tileData.unitOnTile = newUnit;
                        _playerActions.TacticsGrid.GridTiles[index] = tileData;

                        return true;
                    }
                }
            }
            return false;
        }
    }
}