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

        private List<GridIndex> _rangeIndexes = new List<GridIndex>();
        private List<GridIndex> _areaOfEffectIndexes = new List<GridIndex>();

        public override void InitializeAction(PlayerActions playerActions)
        {
            base.InitializeAction(playerActions);

            _playerActions.OnHoveredTileChanged += PlayerActions_OnHoveredTileChanged;
            _playerActions.OnCurrentAbilityChanged += PlayerActions_OnCurrentAbilityChanged;
        }

        private void PlayerActions_OnCurrentAbilityChanged(Ability ability)
        {
            ClearStateFromPreviousList(TileState.IsInAbilityRange, _rangeIndexes);
            ClearStateFromPreviousList(TileState.IsInAoeRange, _areaOfEffectIndexes);

            _currentAbility = ability;

            if(_currentAbility != null && _selectedTileIndex != GridIndex.Invalid())
            {
                ShowAbilityRangePattern();
            }
        }

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_playerActions.TacticsGrid.IsIndexValid(index) && index != _selectedTileIndex)
                _selectedTileIndex = index;
            else
                _selectedTileIndex = GridIndex.Invalid();

            ClearStateFromPreviousList(TileState.IsInAbilityRange, _rangeIndexes);
            ClearStateFromPreviousList(TileState.IsInAoeRange, _areaOfEffectIndexes);

            if (_currentAbility && _selectedTileIndex != GridIndex.Invalid())
            {
                ShowAbilityRangePattern();
                return true;
            }

            return false;
        }

        private void ShowAbilityRangePattern()
        {
            _rangeIndexes = _playerActions.CombatSystem.GetAbilityRange(_selectedTileIndex, _currentAbility.RangeData);
            if (_currentAbility.RangeData.lineOfSightData.requireLineOfSight)
                _rangeIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_selectedTileIndex, _rangeIndexes, _currentAbility.RangeData.lineOfSightData.height);
            SetTileStateOnList(TileState.IsInAbilityRange, _rangeIndexes);
        }
        private void ShowAbilityAreaOfEffectPattern()
        {
            _areaOfEffectIndexes = _playerActions.CombatSystem.GetAbilityRange(_hoveredTileIndex, _currentAbility.AreaOfEffectData);
            if (_currentAbility.AreaOfEffectData.lineOfSightData.requireLineOfSight)
                _areaOfEffectIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_hoveredTileIndex, _areaOfEffectIndexes, _currentAbility.AreaOfEffectData.lineOfSightData.height);
            SetTileStateOnList(TileState.IsInAoeRange, _areaOfEffectIndexes);
        }

        private void PlayerActions_OnHoveredTileChanged(GridIndex index)
        {

            ClearStateFromPreviousList(TileState.IsInAoeRange, _areaOfEffectIndexes);

            _hoveredTileIndex = index;

            if (_currentAbility == null)
                return;

            if (_selectedTileIndex == GridIndex.Invalid())
                return;

            if (!_rangeIndexes.Contains(_hoveredTileIndex))
                return;

            ShowAbilityAreaOfEffectPattern();
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

            ClearStateFromPreviousList(TileState.IsInAoeRange, _areaOfEffectIndexes);
            ClearStateFromPreviousList(TileState.IsInAbilityRange, _rangeIndexes);
        }
    }
}