using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class CalculateHeuristicAction : SelectTileAction
	{
        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (!_playerActions.TacticsGrid.IsIndexValid(index))
                return false;

            _playerActions.TacticsGrid.GridPathfinder.ClearNodePool();

            for(int i = 0; i < _playerActions.TacticsGrid.GridTiles.Count; i++)
            {
                GridIndex gridIndex = _playerActions.TacticsGrid.GridTiles.ElementAt(i).Key;
                PathNode gridNode = _playerActions.TacticsGrid.GridPathfinder.CreateAndAddNodeToPool(gridIndex);
                gridNode.heuristicCost = _playerActions.TacticsGrid.GridPathfinder.GetHeuristicCost(gridIndex, index);
            }
            GameObject.Find("[DebugMenu]").GetComponent<DebugTextOnTiles>().UpdateDebugText();

            return true;
        }
    }
}