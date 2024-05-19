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

        private List<GridIndex> _toTargetIndexes = new List<GridIndex>();
        private List<GridIndex> _onTargetTiles = new List<GridIndex>();

        public override void InitializeAction(PlayerActions playerActions)
        {
            base.InitializeAction(playerActions);

            _playerActions.OnHoveredTileChanged += PlayerActions_OnHoveredTileChanged;
            _playerActions.OnCurrentAbilityChanged += PlayerActions_OnCurrentAbilityChanged;
        }

        private void PlayerActions_OnCurrentAbilityChanged(Ability ability)
        {
            ClearStateFromPreviousList(_onTargetTiles);
            ClearStateFromPreviousList(_toTargetIndexes);

            _currentAbility = ability;

            if(_currentAbility != null && _selectedTile != GridIndex.Invalid())
            {
                ShowAbilityRangePattern();
            }
        }

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_playerActions.TacticsGrid.IsIndexValid(index) && index != _selectedTile)
            {
                _selectedTile = index;

                ClearStateFromPreviousList(_toTargetIndexes);
            }
            else
            {
                _currentAbility = null;
                _selectedTile = GridIndex.Invalid();
                ClearStateFromPreviousList(_onTargetTiles);
                ClearStateFromPreviousList(_toTargetIndexes);
            }

            if (_currentAbility)
            {
                ShowAbilityRangePattern();
                return true;
            }

            return false;
        }

        private void ShowAbilityRangePattern()
        {
            _toTargetIndexes = _playerActions.CombatSystem.GetAbilityToTargetRange(_selectedTile, _currentAbility);
            if (_currentAbility.LineOfSightData.requireLineOfSight)
                _toTargetIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_selectedTile, _toTargetIndexes, _currentAbility.LineOfSightData.height);
            SetTileStateToAbilityRange(_toTargetIndexes);
        }

        private void PlayerActions_OnHoveredTileChanged(GridIndex index)
        {
            //if (_currentAbility == null)
            //    return;

            //if (_selectedTile == GridIndex.Invalid())
            //{
            //    ClearStateFromPreviousList(_managedTiles);
            //    return;
            //}

            //_hoveredTile = index;

            //ClearStateFromPreviousList(_managedTiles);

            //if (!_playerActions.CombatSystem.HasLineOfSight(_selectedTile, _hoveredTile, 1f))
            //    return;

            //_managedTiles = GetTilesForTargetPattern(_currentAbility.OnTargetData.rangePattern, _hoveredTile, _currentAbility.OnTargetData.rangeMinMax);
            //SetTileStateToAbilityRange(_managedTiles);
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

        private void OnDestroy()
        {
            _playerActions.OnHoveredTileChanged -= PlayerActions_OnHoveredTileChanged;
            _playerActions.OnCurrentAbilityChanged -= PlayerActions_OnCurrentAbilityChanged;

            ClearStateFromPreviousList(_onTargetTiles);
            ClearStateFromPreviousList(_toTargetIndexes);
        }
    }
}