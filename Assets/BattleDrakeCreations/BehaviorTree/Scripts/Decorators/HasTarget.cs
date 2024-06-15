using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
    public class HasTarget : DecoratorNode
    {
        [SerializeField] private IsSet _isSet;
        public override string title { get => "Has Target?"; }
        public override string description { get => $"IsSet: {_isSet}"; }

        private BlackboardKey _targetUnitKey;
        private Unit _targetUnit;

        protected override void OnStart()
        {
            _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");
            _blackboard.OnValueSet -= Blackboard_OnValueSet;
            _blackboard.OnValueSet += Blackboard_OnValueSet;

            _result = NodeResult.Running;
        }

        private void Blackboard_OnValueSet(BlackboardKey key)
        {
            if (key.Equals(_targetUnitKey))
            {
                if (_blackboard.TryGetValue(_targetUnitKey, out Unit targetUnit))
                {
                    _targetUnit = targetUnit;
                }
            }
        }

        protected override void OnStop()
        {
            _blackboard.OnValueSet -= Blackboard_OnValueSet;
        }

        protected override NodeResult OnEvaluate()
        {
            if (_targetUnit == null && _isSet == IsSet.IsNotSet || _targetUnit != null && _isSet == IsSet.IsSet)
                return _child.Evaluate();
            else
                return NodeResult.Succeeded;
        }
    }
}