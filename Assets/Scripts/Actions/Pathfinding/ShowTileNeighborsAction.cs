using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowTileNeighborsAction : ActionBase
    {
        private List<GridIndex> _neighborList = new List<GridIndex>();

        public override bool ExecuteAction(GridIndex index)
        {
            if(_neighborList.Count > 0)
            {
                _playerActions.TacticsGrid.ClearStateFromTiles(_neighborList, TileState.IsNeighbor);
            }

            if (_playerActions.TacticsGrid.IsIndexValid(index))
            {
                bool includeDiagonals = actionValue == 1;
                _neighborList = _playerActions.TacticsGrid.GridPathfinder.GetTileNeighbors(index, includeDiagonals);

                _playerActions.TacticsGrid.GridTiles.TryGetValue(index, out TileData selectedData);

                for(int i = 0; i < _neighborList.Count; i++)
                {
                    if (_playerActions.TacticsGrid.GridTiles.TryGetValue(_neighborList[i], out TileData tileData))
                    {
                        if (_playerActions.TacticsGrid.IsIndexValid(_neighborList[i]))
                        {
                            if (GridStatics.IsTileTypeWalkable(tileData.tileType))
                            {
                                float heightDifference = Mathf.Abs(tileData.tileMatrix.GetPosition().y - selectedData.tileMatrix.GetPosition().y);
                                if (heightDifference <= _playerActions.TacticsGrid.GridPathfinder.HeightAllowance)
                                {
                                    _playerActions.TacticsGrid.AddStateToTile(_neighborList[i], TileState.IsNeighbor);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}