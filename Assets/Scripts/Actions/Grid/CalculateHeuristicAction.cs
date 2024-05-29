using System.Linq;
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
                _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();
                return false;
            }

            if (!_playerActions.TacticsGrid.IsIndexValid(index))
                return false;

            foreach(var gridTilePair in _playerActions.TacticsGrid.GridTiles)
            {
                GridIndex gridIndex = gridTilePair.Key;
                PathNode gridNode = _playerActions.TacticsGrid.GridPathfinder.CreateAndAddNodeToPool(gridIndex);
                gridNode.heuristicCost = _playerActions.TacticsGrid.GridPathfinder.GetHeuristicCost(gridIndex, index);
                _playerActions.TacticsGrid.GridPathfinder.PathNodePool[gridIndex] = gridNode;
                _playerActions.TacticsGrid.GridPathfinder.OnPathfindingDataUpdated?.Invoke();
            }
            return true;
        }

        private void OnDestroy()
        {
            _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();
        }
    }
}