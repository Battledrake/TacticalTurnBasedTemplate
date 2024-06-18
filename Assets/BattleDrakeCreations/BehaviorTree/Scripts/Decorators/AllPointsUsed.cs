using BattleDrakeCreations.BehaviorTree;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
    public class AllPointsUsed : DecoratorNode
    {
        public override string title { get => "All Points Used"; }
        public override string description { get => $"Current AP: {_currentActionPoints}"; }

        private int _currentActionPoints = 0;
        protected override void OnStart()
        {
            _currentActionPoints = _agent.AbilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints);
            _agent.AbilitySystem.OnAttributeCurrentChanged += AbilitySystem_OnAttributeCurrentChanged;
        }

        private void AbilitySystem_OnAttributeCurrentChanged(AttributeId attribute, int oldValue, int newValue)
        {
            if (attribute == AttributeId.ActionPoints)
            {
                _currentActionPoints = newValue;
            }
        }

        protected override void OnStop()
        {
            _agent.AbilitySystem.OnAttributeCurrentChanged -= AbilitySystem_OnAttributeCurrentChanged;
        }

        protected override NodeResult OnEvaluate()
        {
            _child.Evaluate();

            if (_currentActionPoints > 0)
                return NodeResult.Running;

            return NodeResult.Succeeded;
        }
    }
}