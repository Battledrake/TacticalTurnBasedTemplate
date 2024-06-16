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

            _abilityInUse = true;
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);

            CombatManager.Instance.OnActionEnded += CombatManager_OnActionEnded;

            if (!CombatManager.Instance.UseAbility(_currentAbility, _playerActions.SelectedTile, index))
            {
                CombatManager.Instance.OnActionEnded -= CombatManager_OnActionEnded;
                _abilityInUse = false;
                return false;
            }

            _playerActions.PlayerAbilityBar.SetSelectedAbilityFromIndex(-1);

            return true;
        }

        private bool UnitHasEnoughActionPoints(int amountNeeded = 1)
        {
            if (_playerActions.SelectedUnit)
            {
                AbilitySystem abilitySystem = _playerActions.SelectedUnit.GetComponent<IAbilitySystem>().AbilitySystem;
                if (abilitySystem)
                {
                    return abilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints) >= amountNeeded;
                }
            }
            return false;
        }

        private void CombatManager_OnActionEnded()
        {
            CombatManager.Instance.OnActionEnded -= CombatManager_OnActionEnded;
            _abilityInUse = false;
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            if (!UnitHasEnoughActionPoints())
                return;

            ShowAbilityAreaOfEffectPattern();
        }

        public void SetAbility(Ability ability)
        {
            _currentAbility = ability;
            if (_currentAbility == null || !UnitHasEnoughActionPoints())
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
            List<GridIndex> rangeIndexes = CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _currentAbility.RangeData);
            for (int i = 0; i < rangeIndexes.Count; i++)
            {
                _playerActions.TacticsGrid.AddStateToTile(rangeIndexes[i], TileState.IsInAbilityRange);
            }

            ShowAbilityAreaOfEffectPattern();
        }

        private void ShowAbilityAreaOfEffectPattern()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);

            if (!CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _currentAbility.RangeData).Contains(_playerActions.HoveredTile))
                return;

            List<GridIndex> areaOfEffectIndexes = CombatManager.Instance.GetAbilityRange(_playerActions.HoveredTile, _currentAbility.AreaOfEffectData);

            if (_currentAbility.RangeData.lineOfSightData.requireLineOfSight)
                areaOfEffectIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_playerActions.HoveredTile, areaOfEffectIndexes, _currentAbility.AreaOfEffectData.lineOfSightData.height, _currentAbility.AreaOfEffectData.lineOfSightData.offsetDistance);

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