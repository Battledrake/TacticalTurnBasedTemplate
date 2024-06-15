using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class Cooldown : DecoratorNode
    {
        [SerializeField] private float _cooldown;

        public override string title { get => "Cooldown"; }
        public override string description { get => $"Time Left: {(_hasStarted ? (_startTime + _cooldown) - Time.time : 0):F2}"; }

        private float _startTime = 0f;

        protected override NodeResult OnEvaluate()
        {
            if (Time.time >= _startTime + _cooldown)
                return _child.Evaluate();
            else
                return NodeResult.Running;
        }

        protected override void OnStart()
        {
            _startTime = Time.time;
        }

        protected override void OnStop()
        {
        }
    }
}