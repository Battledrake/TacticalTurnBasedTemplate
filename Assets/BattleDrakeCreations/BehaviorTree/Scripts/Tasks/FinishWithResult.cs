using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class FinishWithResult : TaskNode
    {
        [SerializeField] private NodeResult _returnResult;

        public override string title { get => $"Finish With Result"; }
        public override string description { get => $"Return: {_returnResult}"; }

        protected override NodeResult OnEvaluate()
        {
            return _returnResult;
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }
    }
}