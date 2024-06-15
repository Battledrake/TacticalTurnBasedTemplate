using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class FindPathAction : ActionBase
    {
        public override bool ExecuteAction(GridIndex index)
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);

            GridIndex previousTile = _playerActions.SelectedTile;
            if (previousTile != index)
            {
                float pathLength = actionValue;

                PathParams pathParams = _playerActions.TacticsGrid.Pathfinder.CreateDefaultPathParams(pathLength);
                    
                PathfindingResult result = _playerActions.TacticsGrid.Pathfinder.FindPath(_playerActions.SelectedTile, index, pathParams);

                _playerActions.TacticsGrid.Pathfinder.OnPathfindingCompleted?.Invoke();

                if (result.Result != PathResult.SearchFail)
                {
                    List<GridIndex> pathIndexes = PathfindingStatics.ConvertPathNodesToGridIndexes(result.Path);
                    for (int i = 0; i < pathIndexes.Count; i++)
                    {
                        _playerActions.TacticsGrid.AddStateToTile(pathIndexes[i], TileState.IsInPath);
                    }
                    return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInPath);
        }
    }
}