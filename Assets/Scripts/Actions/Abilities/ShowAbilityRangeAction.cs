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

            _playerActions.OnCurrentAbilityChanged += PlayerActions_OnCurrentAbilityChanged;
            _playerActions.OnSelectedTileChanged += PlayerActions_OnSelectedTileChanged;
        }

        private void PlayerActions_OnSelectedTileChanged(GridIndex index)
        {
            ClearStateFromPreviousList(TileState.IsInAbilityRange);
            ClearStateFromPreviousList(TileState.IsInAoeRange);

            _selectedTileIndex = index;

            if (_currentAbility != null && index != GridIndex.Invalid())
            {
                ShowAbilityRangePattern();
            }
        }

        private void PlayerActions_OnCurrentAbilityChanged(Ability ability)
        {
            ClearStateFromPreviousList(TileState.IsInAbilityRange);
            ClearStateFromPreviousList(TileState.IsInAoeRange);

            _currentAbility = ability;
            _selectedTileIndex = _playerActions.SelectedTile;

            if (_currentAbility != null && _selectedTileIndex != GridIndex.Invalid())
            {
                ShowAbilityRangePattern();
            }
        }

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            _selectedTileIndex = _playerActions.SelectedTile;

            ClearStateFromPreviousList(TileState.IsInAbilityRange);
            ClearStateFromPreviousList(TileState.IsInAoeRange);

            if (_currentAbility && _selectedTileIndex != GridIndex.Invalid())
            {
                ShowAbilityRangePattern();
                ShowAbilityAreaOfEffectPattern();
                return true;
            }

            return false;
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            ClearStateFromPreviousList(TileState.IsInAoeRange);

            _hoveredTileIndex = hoveredIndex;

            if (_currentAbility == null)
                return;

            if (_selectedTileIndex == GridIndex.Invalid())
                return;

            ShowAbilityAreaOfEffectPattern();
        }

        private void ShowAbilityRangePattern()
        {
            _rangeIndexes = CombatManager.Instance.GetAbilityRange(_selectedTileIndex, _currentAbility.RangeData, _playerActions.SelectedUnit);

            if (_currentAbility.RangeData.lineOfSightData.requireLineOfSight)
                _rangeIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_selectedTileIndex, _rangeIndexes, _currentAbility.RangeData.lineOfSightData.height);

            SetTileStateOnList(TileState.IsInAbilityRange, _rangeIndexes);
        }
        private void ShowAbilityAreaOfEffectPattern()
        {

            if (_currentAbility.AreaOfEffectData.rangePattern != AbilityRangePattern.Movement)
            {
                if (!_rangeIndexes.Contains(_hoveredTileIndex))
                    return;

                _areaOfEffectIndexes = CombatManager.Instance.GetAbilityRange(_hoveredTileIndex, _currentAbility.AreaOfEffectData);
            }
            else
            {
                AbilityRangeData sprintRange = _currentAbility.RangeData;
                sprintRange.rangeMinMax = sprintRange.rangeMinMax * new Vector2Int(0, 2);
                _areaOfEffectIndexes = CombatManager.Instance.GetAbilityRange(_selectedTileIndex, sprintRange);

                for (int i = 0; i < _rangeIndexes.Count; i++)
                {
                    _areaOfEffectIndexes.Remove(_rangeIndexes[i]);
                }
            }

            if (_currentAbility.AreaOfEffectData.lineOfSightData.requireLineOfSight)
                _areaOfEffectIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_hoveredTileIndex, _areaOfEffectIndexes, _currentAbility.AreaOfEffectData.lineOfSightData.height);

            SetTileStateOnList(TileState.IsInAoeRange, _areaOfEffectIndexes);
        }

        private void ClearStateFromPreviousList(TileState tileState)
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(tileState);
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
            _playerActions.OnCurrentAbilityChanged -= PlayerActions_OnCurrentAbilityChanged;
            _playerActions.OnSelectedTileChanged -= PlayerActions_OnSelectedTileChanged;

            ClearStateFromPreviousList(TileState.IsInAoeRange);
            ClearStateFromPreviousList(TileState.IsInAbilityRange);
        }
    }
}