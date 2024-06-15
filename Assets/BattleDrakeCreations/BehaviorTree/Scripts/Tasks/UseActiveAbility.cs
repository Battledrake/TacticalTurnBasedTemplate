using BattleDrakeCreations.BehaviorTree;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
    public class UseActiveAbility : TaskNode
    {
        public override string title { get => "Use Active Ability"; }
        public override string description { get => ""; }

        private BlackboardKey _activeAbilityKey;
        private BlackboardKey _targetUnitKey;

        private bool _isUsingAbility = false;
        private bool _isAbilityCompleted = false;

        protected override void OnStart()
        {
            _activeAbilityKey = _blackboard.GetOrRegisterKey("ActiveAbility");
            _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");

            _result = NodeResult.Running;
            _isUsingAbility = false;
            _isAbilityCompleted = false;
        }

        protected override NodeResult OnEvaluate()
        {
            if (_isUsingAbility)
                return NodeResult.Running;

            if (_isAbilityCompleted)
                return NodeResult.Succeeded;

            _blackboard.TryGetValue(_activeAbilityKey, out Ability activeAbility);
            _blackboard.TryGetValue(_targetUnitKey, out Unit targetUnit);

            if (activeAbility == null || targetUnit == null)
                return NodeResult.Failed;

            activeAbility.OnAbilityEnded += Ability_OnAbilityEnded;
            if (!CombatManager.Instance.TryActivateAbility(activeAbility, _agent.Unit.GridIndex, targetUnit.GridIndex))
            {
                activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;
                return NodeResult.Failed;
            }
            _isUsingAbility = true;
            return NodeResult.Running;
        }

        private void Ability_OnAbilityEnded(Ability ability)
        {
            ability.OnAbilityEnded -= Ability_OnAbilityEnded;
            _isUsingAbility = false;
            _isAbilityCompleted = true;
        }

        protected override void OnStop()
        {
        }
    }
}