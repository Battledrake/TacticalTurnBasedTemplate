using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowAbilityRangeAction : SelectTileAndUnitAction
    {
        private Ability _currentAbility = null;
        private GridIndex _selectedTile = GridIndex.Invalid();
        private GridIndex _hoveredTile = GridIndex.Invalid();

        private List<GridIndex> _managedTiles = new List<GridIndex>();

        public override void InitializeAction(PlayerActions playerActions)
        {
            base.InitializeAction(playerActions);

            _playerActions.OnHoveredTileChanged += PlayerActions_OnHoveredTileChanged;
        }



        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_playerActions.TacticsGrid.IsIndexValid(index) && index != _selectedTile)
            {
                _currentAbility = _playerActions.CurrentAbility;
                _selectedTile = index;
            }
            else
            {
                _currentAbility = null;
                _selectedTile = GridIndex.Invalid();
                ClearStateFromPreviousList(_managedTiles);
            }

            return true;
        }

        private void PlayerActions_OnHoveredTileChanged(GridIndex index)
        {
            if (_currentAbility == null)
                return;

            if (_selectedTile == GridIndex.Invalid())
            {
                ClearStateFromPreviousList(_managedTiles);
                return;
            }

            _hoveredTile = index;

            ClearStateFromPreviousList(_managedTiles);

            if (!_playerActions.CombatSystem.HasLineOfSight(_selectedTile, _hoveredTile, 1f))
                return;

            if (PathfindingStatics.GetChebyshevDistance(_selectedTile, _hoveredTile) > _currentAbility._rangeMinMax.y)
                return;

            _managedTiles = GetTilesForTargetPattern(_currentAbility._targetPattern, _hoveredTile);
            SetTileStateToAbilityRange(_managedTiles);
        }

        private List<GridIndex> GetTilesForToTargetType(ToTargetPattern toTargetPattern, GridIndex selectedIndex, GridIndex currentHoveredTile)
        {
            PathFilter pathFilter = _playerActions.TacticsGrid.GridPathfinder.CreateDefaultPathFilter(actionValue);
            PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(selectedIndex, pathFilter);
            if (pathResult.Result != PathResult.SearchFail)
            {
                return pathResult.Path;
            }
            return new List<GridIndex>();
        }

        private List<GridIndex> GetTilesForTargetPattern(TargetPattern targetPattern, GridIndex targetIndex)
        {
            switch (targetPattern)
            {
                case TargetPattern.Single:
                    return new List<GridIndex> { targetIndex };
                case TargetPattern.AOE:
                    {
                        PathFilter pathFilter = _playerActions.TacticsGrid.GridPathfinder.CreateDefaultPathFilter(actionValue);
                        PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(targetIndex, pathFilter);
                        if (pathResult.Result != PathResult.SearchFail)
                        {
                            return _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(targetIndex, pathResult.Path, 1f);
                        }
                    }
                    break;
            }
            return new List<GridIndex>();
        }
        private void ClearStateFromPreviousList(List<GridIndex> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                _playerActions.TacticsGrid.RemoveStateFromTile(tiles[i], TileState.IsInAbilityRange);
            }
        }
        private void SetTileStateToAbilityRange(List<GridIndex> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(tiles[i], TileState.IsInAbilityRange);
            }
        }

        private void ShowAbilityRangePattern()
        {
            //_currentAbility.MinMaxRange = values
            //ClearTileStateIsSpellRange
            //IsTileValid
            //CombatSystem.GetAbilityRange(index, List<GridIndex> tilesRetrieved)
            //
        }

        private void OnDestroy()
        {
            _playerActions.OnHoveredTileChanged -= PlayerActions_OnHoveredTileChanged;

            ClearStateFromPreviousList(_managedTiles);
        }
    }
}