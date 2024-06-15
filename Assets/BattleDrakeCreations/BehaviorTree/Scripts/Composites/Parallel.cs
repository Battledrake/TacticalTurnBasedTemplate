using System.Collections.Generic;

namespace BattleDrakeCreations.BehaviorTree
{
    public class Parallel : CompositeNode
    {
        public override string title { get => "Parallel"; }

        private List<NodeResult> _childrenLeftToEvaluate = new();
        protected override NodeResult OnEvaluate()
        {
            bool stillRunning = false;
            for(int i = 0; i < _childrenLeftToEvaluate.Count; i++)
            {
                if (_childrenLeftToEvaluate[i] == NodeResult.Running)
                {
                    NodeResult result = _children[i].Evaluate();
                    if(result == NodeResult.Failed)
                    {
                        AbortRunningChildren();
                        return NodeResult.Failed;
                    }

                    if(result == NodeResult.Running)
                    {
                        stillRunning = true;
                    }

                    _childrenLeftToEvaluate[i] = result;
                }
            }

            return stillRunning ? NodeResult.Running : NodeResult.Succeeded;
        }

        protected override void OnStart()
        {
            _childrenLeftToEvaluate.Clear();
            _children.ForEach(r =>
            {
                _childrenLeftToEvaluate.Add(NodeResult.Running);
            });
        }

        protected override void OnStop()
        {
        }

        private void AbortRunningChildren()
        {
            for(int i = 0; i < _childrenLeftToEvaluate.Count; i++)
            {
                if (_childrenLeftToEvaluate[i] == NodeResult.Running)
                {
                    _children[i].Abort();
                }
            }
        }
    }
}