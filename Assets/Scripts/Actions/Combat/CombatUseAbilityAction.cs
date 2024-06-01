using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatUseAbilityAction : ActionBase
	{
        private Ability _currentAbility;
        AbilityRangeData _rangeData;
        AbilityRangeData _areaOfEffectData;

        public override bool ExecuteAction(GridIndex index)
        {
            if(CombatManager.Instance.TryActivateAbility(_currentAbility, _playerActions.SelectedUnit, _playerActions.SelectedTile, index))
            {
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
                CombatManager.Instance.OnAbilityBehaviorComplete += CombatManager_OnAbilityBehaviorComplete;
            }
            return true;
        }

        private void CombatManager_OnAbilityBehaviorComplete()
        {
            CombatManager.Instance.OnAbilityBehaviorComplete -= CombatManager_OnAbilityBehaviorComplete;
            _playerActions.PlayerAbilityBar.SetSelectedAbilityFromIndex(-1);
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            ShowAbilityAreaOfEffectPattern();
        }

        public void SetAbility(Ability ability)
        {
            _currentAbility = ability;
            if(_currentAbility == null)
            {
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
            }
            else
            {
                _rangeData = ability.GetRangeData();
                _areaOfEffectData = ability.GetAreaOfEffectData();
                ShowAbilityRangePattern();
            }
        }

        private void ShowAbilityRangePattern()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
            List<GridIndex> rangeIndexes = CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _rangeData);
            for(int i = 0; i < rangeIndexes.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(rangeIndexes[i], TileState.IsInAbilityRange);
            }

            ShowAbilityAreaOfEffectPattern();
        }

        private void ShowAbilityAreaOfEffectPattern()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);

            if (!CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _rangeData).Contains(_playerActions.HoveredTile))
                return;

            List<GridIndex> _areaOfEffectIndexes = CombatManager.Instance.GetAbilityRange(_playerActions.HoveredTile, _currentAbility.GetAreaOfEffectData());

            if (_currentAbility.GetRangeData().lineOfSightData.requireLineOfSight)
                _areaOfEffectIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_playerActions.HoveredTile, _areaOfEffectIndexes, _currentAbility.GetAreaOfEffectData().lineOfSightData.height, _currentAbility.GetAreaOfEffectData().lineOfSightData.offsetDistance);

            for(int i = 0; i < _areaOfEffectIndexes.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(_areaOfEffectIndexes[i], TileState.IsInAoeRange);
            }

        }

        private void OnDisable()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
        }
    }
}