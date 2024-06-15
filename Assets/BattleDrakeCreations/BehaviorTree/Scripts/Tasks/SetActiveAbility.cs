using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
    public class SetActiveAbility : TaskNode
    {
        [SerializeField] private AbilityId _abilityId;
        [Tooltip("Give the unit the ability if it doesn't have it")]
        [SerializeField] private bool _giveAbility;

        public override string title { get => "Set Active Ability"; }
        public override string description { get => $"Ability: {_abilityId}"; }

        private BlackboardKey _activeAbilityKey;

        protected override void OnStart()
        {
            _activeAbilityKey = _blackboard.GetOrRegisterKey("ActiveAbility");
        }

        protected override void OnStop()
        {
        }

        protected override NodeResult OnEvaluate()
        {
            Ability ability = _agent.AbilitySystem.GetAbility(_abilityId);
            if (ability == null)
            {
                if (!_giveAbility)
                {
                    return NodeResult.Failed;
                }
                else
                {
                    _agent.AbilitySystem.AddAbility(_abilityId);
                    ability = _agent.AbilitySystem.GetAbility(_abilityId);
                }
            }

            if (ability)
            {
                _blackboard.SetValue(_activeAbilityKey, ability);
                return NodeResult.Succeeded;
            }

            return NodeResult.Failed;
        }
    }
}