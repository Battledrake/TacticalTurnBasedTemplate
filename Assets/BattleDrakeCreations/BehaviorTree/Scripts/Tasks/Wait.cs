using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class Wait : TaskNode
    {
        [SerializeField] private float _waitTime = 1f;

        public override string title { get => "Wait"; }
        public override string description { get => $"Wait: {_waitTime:F2}, Remaining: {Mathf.Clamp((_startTime + _waitTime) - Time.time, 0, _startTime + _waitTime):F2}"; }

        private float _startTime;
        protected override void OnStart()
        {
            _startTime = Time.time;
        }

        protected override NodeResult OnEvaluate()
        {
            if (Time.time >= _startTime + _waitTime)
                _result = NodeResult.Succeeded;
            else
                _result = NodeResult.Running;

            return _result;
        }

        protected override void OnStop()
        {
        }
    }
}
