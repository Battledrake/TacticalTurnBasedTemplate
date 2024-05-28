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
                return CombatManager.Instance.TryActivateAbility(_ability, _playerActions.SelectedUnit, _playerActions.SelectedTile, index);
            }
            return false;
        }
    }
}