using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CalculateHeuristicAction : SelectTileAction
	{
        public override bool ExecuteAction(GridIndex index)
        {
            if (!base.ExecuteAction(index))
            {
                _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();
                return false;
            }

            if (!_playerActions.TacticsGrid.IsIndexValid(index))
                return false;

            for(int i = 0; i < _playerActions.TacticsGrid.GridTiles.Count; i++)
            {
                GridIndex gridIndex = _playerActions.TacticsGrid.GridTiles.ElementAt(i).Key;
                PathNode gridNode = _playerActions.TacticsGrid.GridPathfinder.CreateAndAddNodeToPool(gridIndex);
                gridNode.heuristicCost = _playerActions.TacticsGrid.GridPathfinder.GetHeuristicCost(gridIndex, index);
                _playerActions.TacticsGrid.GridPathfinder.OnPathfindingDataUpdated?.Invoke(gridIndex);
            }
            return true;
        }

        private void OnDestroy()
        {
            _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();
        }
    }
}