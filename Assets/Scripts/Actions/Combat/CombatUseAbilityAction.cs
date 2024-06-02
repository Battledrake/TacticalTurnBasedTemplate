using System;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatUseAbilityAction : ActionBase
    {
        private Ability _currentAbility;

        private bool _abilityInUse = false;

        public override bool ExecuteAction(GridIndex index)
        {
            if (_abilityInUse)
                return false;

            if (CombatManager.Instance.TryActivateAbility(_currentAbility, _playerActions.SelectedTile, index))
            {
                _abilityInUse = true;
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);

                CombatManager.Instance.OnAbilityUseCompleted += CombatManager_OnAbilityBehaviorComplete;
                return true;
            }
            return false;
        }

        private bool UnitHasEnoughAbilityPoints(int amountNeeded = 1)
        {
            if (_playerActions.SelectedUnit)
            {
                AbilitySystem abilitySystem = _playerActions.SelectedUnit.GetComponent<IAbilitySystem>().GetAbilitySystem();
                if (abilitySystem)
                {
                    return abilitySystem.CurrentAbilityPoints >= amountNeeded;
                }
            }
            return false;
        }

        private void CombatManager_OnAbilityBehaviorComplete()
        {
            _abilityInUse = false;
            CombatManager.Instance.OnAbilityUseCompleted -= CombatManager_OnAbilityBehaviorComplete;
            _playerActions.PlayerAbilityBar.SetSelectedAbilityFromIndex(-1);
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            if (!UnitHasEnoughAbilityPoints())
                return;

            ShowAbilityAreaOfEffectPattern();
        }

        public void SetAbility(Ability ability)
        {
            _currentAbility = ability;
            if (_currentAbility == null || !UnitHasEnoughAbilityPoints())
            {
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
            }
            else
            {
                ShowAbilityRangePattern();
            }
        }

        private void ShowAbilityRangePattern()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
            List<GridIndex> rangeIndexes = CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _currentAbility.GetRangeData());
            for (int i = 0; i < rangeIndexes.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(rangeIndexes[i], TileState.IsInAbilityRange);
            }

            ShowAbilityAreaOfEffectPattern();
        }

        private void ShowAbilityAreaOfEffectPattern()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);

            if (!CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _currentAbility.GetRangeData()).Contains(_playerActions.HoveredTile))
                return;

            List<GridIndex> areaOfEffectIndexes = CombatManager.Instance.GetAbilityRange(_playerActions.HoveredTile, _currentAbility.GetAreaOfEffectData());

            if (_currentAbility.GetRangeData().lineOfSightData.requireLineOfSight)
                areaOfEffectIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_playerActions.HoveredTile, areaOfEffectIndexes, _currentAbility.GetAreaOfEffectData().lineOfSightData.height, _currentAbility.GetAreaOfEffectData().lineOfSightData.offsetDistance);

            for (int i = 0; i < areaOfEffectIndexes.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(areaOfEffectIndexes[i], TileState.IsInAoeRange);
            }

        }

        private void OnDisable()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
        }
    }
}