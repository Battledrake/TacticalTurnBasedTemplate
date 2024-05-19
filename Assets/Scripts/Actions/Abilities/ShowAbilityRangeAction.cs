using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShowAbilityRangeAction : SelectTileAndUnitAction
    {
        private Ability _currentAbility = null;
        private GridIndex _selectedTileIndex = GridIndex.Invalid();
        private GridIndex _hoveredTileIndex = GridIndex.Invalid();

        private List<GridIndex> _toTargetIndexes = new List<GridIndex>();
        private List<GridIndex> _onTargetIndexes = new List<GridIndex>();

        public override void InitializeAction(PlayerActions playerActions)
        {
            base.InitializeAction(playerActions);

            _playerActions.OnHoveredTileChanged += PlayerActions_OnHoveredTileChanged;
            _playerActions.OnCurrentAbilityChanged += PlayerActions_OnCurrentAbilityChanged;
        }

        private void PlayerActions_OnCurrentAbilityChanged(Ability ability)
        {
            ClearStateFromPreviousList(TileState.IsInToTargetRange, _toTargetIndexes);
            ClearStateFromPreviousList(TileState.IsInOnTargetRange, _onTargetIndexes);

            _currentAbility = ability;

            if(_currentAbility != null && _selectedTileIndex != GridIndex.Invalid())
            {
                ShowAbilityToTargetRangePattern();
            }
        }

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_playerActions.TacticsGrid.IsIndexValid(index) && index != _selectedTileIndex)
            {
                _selectedTileIndex = index;
            }
            else
            {
                _currentAbility = null;
                _selectedTileIndex = GridIndex.Invalid();
            }

            ClearStateFromPreviousList(TileState.IsInToTargetRange, _toTargetIndexes);
            ClearStateFromPreviousList(TileState.IsInOnTargetRange, _onTargetIndexes);

            if (_currentAbility)
            {
                ShowAbilityToTargetRangePattern();
                return true;
            }

            return false;
        }

        private void ShowAbilityToTargetRangePattern()
        {
            _toTargetIndexes = _playerActions.CombatSystem.GetAbilityRange(_selectedTileIndex, _currentAbility.ToTargetData);
            if (_currentAbility.ToTargetData.lineOfSightData.requireLineOfSight)
                _toTargetIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_selectedTileIndex, _toTargetIndexes, _currentAbility.ToTargetData.lineOfSightData.height);
            SetTileStateOnList(TileState.IsInToTargetRange, _toTargetIndexes);
        }
        private void ShowAbilityOnTargetRangePattern()
        {
            _onTargetIndexes = _playerActions.CombatSystem.GetAbilityRange(_hoveredTileIndex, _currentAbility.OnTargetData);
            if (_currentAbility.OnTargetData.lineOfSightData.requireLineOfSight)
                _onTargetIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_hoveredTileIndex, _onTargetIndexes, _currentAbility.OnTargetData.lineOfSightData.height);
            SetTileStateOnList(TileState.IsInOnTargetRange, _onTargetIndexes);
        }

        private void PlayerActions_OnHoveredTileChanged(GridIndex index)
        {

            ClearStateFromPreviousList(TileState.IsInOnTargetRange, _onTargetIndexes);

            _hoveredTileIndex = index;

            if (_currentAbility == null)
                return;

            if (_selectedTileIndex == GridIndex.Invalid())
                return;

            if (!_toTargetIndexes.Contains(_hoveredTileIndex))
                return;

            ShowAbilityOnTargetRangePattern();
        }

        private void ClearStateFromPreviousList(TileState tileState, List<GridIndex> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                _playerActions.TacticsGrid.RemoveStateFromTile(tiles[i], tileState);
            }
        }
        private void SetTileStateOnList(TileState tileState, List<GridIndex> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(tiles[i], tileState);
            }
        }

        private void OnDestroy()
        {
            _playerActions.OnHoveredTileChanged -= PlayerActions_OnHoveredTileChanged;
            _playerActions.OnCurrentAbilityChanged -= PlayerActions_OnCurrentAbilityChanged;

            ClearStateFromPreviousList(TileState.IsInOnTargetRange, _onTargetIndexes);
            ClearStateFromPreviousList(TileState.IsInToTargetRange, _toTargetIndexes);
        }
    }
}