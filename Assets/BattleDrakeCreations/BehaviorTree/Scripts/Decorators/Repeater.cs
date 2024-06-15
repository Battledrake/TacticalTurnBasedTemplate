using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class Repeater : DecoratorNode
    {
        [Tooltip("Set the number of times the child node is called.")]
        [SerializeField] private int _numberOfLoops = 3;
        [SerializeField] private bool _isInfinite = false;

        public override string title { get => "Repeater"; }
        public override string description { get => $"Loops: {(_isInfinite ? "infinite":_remainingExecutions)}"; }

        private int _remainingExecutions = -1;
        protected override NodeResult OnEvaluate()
        {
            _child.Evaluate();

            bool shouldLoop = false;
            if (_isInfinite)
            {
                shouldLoop = true;
            }
            else
            {
                if (_remainingExecutions > 0)
                {
                    _remainingExecutions--;
                }
                shouldLoop = _remainingExecutions > 0;
            }

            if (shouldLoop)
                return NodeResult.Running;
            else
                return NodeResult.Succeeded;
        }

        protected override void OnStart()
        {
            _remainingExecutions = _numberOfLoops;
        }

        protected override void OnStop()
        {
        }
    }
}
