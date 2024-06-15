using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class LogBlackboard : TaskNode
    {
        public override string title { get => "Log Blackboard"; }
        public override string description { get => $"# of Keys: {_blackboard?.EntryCount}"; }

        protected override NodeResult OnEvaluate()
        {
            if (_blackboard != null)
            {
                _blackboard.Debug();
                return NodeResult.Succeeded;
            }
            else
            {
                Debug.Log("What?");
                return NodeResult.Failed;
            }
        }

        protected override void OnStart()
        {
            _result = NodeResult.Running;
        }

        protected override void OnStop()
        {
        }
    }
}