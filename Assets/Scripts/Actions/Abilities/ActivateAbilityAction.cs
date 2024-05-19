using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ActivateAbilityAction : ActionBase
    {
        [SerializeField] private Ability _testAbility;
        public override bool ExecuteAction(GridIndex index)
        {
            if (!_playerActions.TacticsGrid.IsIndexValid(_playerActions.SelectedTile) || !_playerActions.TacticsGrid.IsIndexValid(index))
                return false;

            if (_playerActions.CurrentAbility)
            {
                _testAbility = _playerActions.CurrentAbility;
            }

            if (_testAbility != null)
            {
                return _playerActions.CombatSystem.TryActivateAbility(_testAbility, _playerActions.SelectedTile, index);
            }
            return false;
        }
    }
}