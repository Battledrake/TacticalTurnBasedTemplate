using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class RootNode : BTNode
    {
        public override string title { get => "Root Node"; }

        public BTNode ChildNode { get => _childNode; set => _childNode = value; }


        [HideInInspector] [SerializeField] private BTNode _childNode;

        protected override NodeResult OnEvaluate()
        {
            return _childNode.Evaluate();
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        public override BTNode Clone()
        {
            RootNode node = Instantiate(this);
            node._childNode = node._childNode.Clone();
            return node;
        }
    }
}
