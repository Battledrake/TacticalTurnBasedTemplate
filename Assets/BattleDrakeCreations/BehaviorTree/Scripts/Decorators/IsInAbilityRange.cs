using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
    public class IsInAbilityRange : DecoratorNode
    {
        [SerializeField] private IsSet _isSet;

        public override string title { get => "Is In Ability Range?"; }
        public override string description { get => $"IsSet: {_isSet}"; }

        private BlackboardKey _targetUnitKey;
        private BlackboardKey _activeAbilityKey;
        protected override void OnStart()
        {
            _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");
            _activeAbilityKey = _blackboard.GetOrRegisterKey("ActiveAbility");

            _result = NodeResult.Running;
        }

        protected override void OnStop()
        {
        }

        protected override NodeResult OnEvaluate()
        {

            _blackboard.TryGetValue(_targetUnitKey, out Unit targetUnit);
            _blackboard.TryGetValue(_activeAbilityKey, out Ability ability);

            if (ability == null || targetUnit == null)
                return NodeResult.Failed;

            if (CombatManager.Instance.GetAbilityRange(_agent.Unit.GridIndex, ability.RangeData).Contains(targetUnit.GridIndex))
            {
                return _isSet == IsSet.IsSet ? _child.Evaluate() : NodeResult.Failed;
            }
            else
            {
                return _isSet == IsSet.IsNotSet ? _child.Evaluate() : NodeResult.Failed;
            }
        }
    }
}