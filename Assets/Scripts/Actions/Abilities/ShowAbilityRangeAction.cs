using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowAbilityRangeAction : SelectTileAndUnitAction
    {
        [SerializeField] private Ability _currentAbility;
        private bool _isActive;
        private GridIndex _selectedTile = GridIndex.Invalid();
        private GridIndex _lastHoveredTile = GridIndex.Invalid();

        private List<GridIndex> _managedTiles = new List<GridIndex>();

        public override void InitializeAction(PlayerActions playerActions)
        {
            base.InitializeAction(playerActions);

            _isActive = true;
        }

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_playerActions.TacticsGrid.IsIndexValid(index) && index != _selectedTile)
                _selectedTile = index;
            else
                _selectedTile = GridIndex.Invalid();

            return true;
        }

        private void Update()
        {
            if (!_isActive)
                return;
            if (_currentAbility == null)
                return;

            //if (_currentAbility._targetType == TargetType.Self)
            //    if (_playerActions.SelectedTile != _selectedTile)
            //        return;

            if (_selectedTile == GridIndex.Invalid())
            {
                ClearStateFromPreviousList(_managedTiles);
                return;
            }

            if (_lastHoveredTile != _playerActions.HoveredTile)
            {
                _lastHoveredTile = _playerActions.HoveredTile;
                ClearStateFromPreviousList(_managedTiles);

                PathFilter pathFilter = _playerActions.TacticsGrid.GridPathfinder.CreateDefaultPathFilter(_currentAbility._rangeMinMax.y);
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindPath(_selectedTile, _lastHoveredTile, pathFilter);
                if(pathResult.Result == PathResult.SearchSuccess)
                {
                    _managedTiles = GetTilesForTargetPattern(TargetPattern.AOE, _lastHoveredTile);
                    SetTileStateToAbilityRange(_managedTiles);
                }
            }
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
            PathFilter pathFilter = _playerActions.TacticsGrid.GridPathfinder.CreateDefaultPathFilter(actionValue);
            PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(targetIndex, pathFilter);
            if (pathResult.Result != PathResult.SearchFail)
            {
                return pathResult.Path;
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
    }
}