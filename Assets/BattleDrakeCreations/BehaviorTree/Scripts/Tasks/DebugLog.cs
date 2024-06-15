using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class DebugLog : TaskNode
    {
        [SerializeField] private string _message;

        public override string title { get => "Debug Log"; }
        public override string description { get => $"Log: {_message}"; }

        protected override void OnStart()
        {
        }

        protected override NodeResult OnEvaluate()
        {
            if (!string.IsNullOrEmpty(_message))
            {
                Debug.Log(_message);
                return NodeResult.Succeeded;
            }
            else
            {
                return NodeResult.Failed;
            }
        }

        protected override void OnStop()
        {
        }
    }
}
