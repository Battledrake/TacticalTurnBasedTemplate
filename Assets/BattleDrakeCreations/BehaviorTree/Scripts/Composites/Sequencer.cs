
namespace BattleDrakeCreations.BehaviorTree
{
    public class Sequencer : CompositeNode
    {
        public override string title { get => "Sequencer"; }
        public override string description { get => $"Active Index: {_currentChild}"; }

        private int _currentChild;

        protected override void OnStart()
        {
            _currentChild = 0;
            _result = NodeResult.Running;
        }

        protected override NodeResult OnEvaluate()
        {
            BTNode child = _children[_currentChild];
            switch (child.Evaluate())
            {
                case NodeResult.Succeeded:
                    _currentChild++;
                    break;
                case NodeResult.Failed:
                    _result = NodeResult.Failed;
                    break;
                case NodeResult.Running:
                    _result = NodeResult.Running;
                    break;
            }

            if (_currentChild >= _children.Count)
                _result = NodeResult.Succeeded;

            return _result;
        }

        protected override void OnStop()
        {
        }
    }
}
