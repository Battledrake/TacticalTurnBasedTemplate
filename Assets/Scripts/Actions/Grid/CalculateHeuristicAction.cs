using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CalculateHeuristicAction : SelectTileAndUnitAction
    {
        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (index != _playerActions.SelectedTile)
            {
                _playerActions.TacticsGrid.Pathfinder.ClearNodePool();
                return false;
            }

            if (!_playerActions.TacticsGrid.IsIndexValid(index))
                return false;

            foreach (KeyValuePair<GridIndex, TileData> gridTilePair in _playerActions.TacticsGrid.GridTiles)
            {
                GridIndex gridIndex = gridTilePair.Key;
                PathNode gridNode = _playerActions.TacticsGrid.Pathfinder.CreateAndAddNodeToPool(gridIndex);
                gridNode.heuristicCost = _playerActions.TacticsGrid.Pathfinder.GetHeuristicCost(gridIndex, index);
                _playerActions.TacticsGrid.Pathfinder.PathNodePool[gridIndex] = gridNode;
                _playerActions.TacticsGrid.Pathfinder.OnPathfindingDataUpdated?.Invoke();
            }
            return true;
        }

        private void OnDestroy()
        {
            _playerActions.TacticsGrid.Pathfinder.ClearNodePool();
        }
    }
}