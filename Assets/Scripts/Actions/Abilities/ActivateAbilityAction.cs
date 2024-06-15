using System;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ActivateAbilityAction : ActionBase
    {
        private Ability _ability = null;

        public override bool ExecuteAction(GridIndex index)
        {
            if (!_playerActions.TacticsGrid.IsIndexValid(_playerActions.SelectedTile) || !_playerActions.TacticsGrid.IsIndexValid(index))
                return false;

            _ability = _playerActions.CurrentAbility;

            if (_ability != null)
            {
                if (!CombatManager.Instance.GetAbilityRange(_playerActions.SelectedTile, _ability.RangeData).Contains(index)) return false;

                AbilityActivationData activationData;
                activationData.tacticsGrid = _playerActions.TacticsGrid;
                activationData.originIndex = _playerActions.SelectedTile;
                activationData.targetIndex = index;
                return _ability.TryActivateAbility(activationData);
            }
            return false;
        }
    }
}