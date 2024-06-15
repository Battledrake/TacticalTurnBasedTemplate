
namespace BattleDrakeCreations.BehaviorTree
{
    public class Selector : CompositeNode
    {
        public override string title { get => "Selector"; }

        private int _currentIndex = -1;
        protected override NodeResult OnEvaluate()
        {
            switch (_children[_currentIndex].Evaluate())
            {
                case NodeResult.Succeeded:
                    return NodeResult.Succeeded;
                case NodeResult.Failed:
                    _currentIndex++;
                    break;
                case NodeResult.Running:
                    break;
            }
            if(_currentIndex >= _children.Count)
            {
                _currentIndex = 0;
                return NodeResult.Failed;
            }
            return NodeResult.Running;
        }

        protected override void OnStart()
        {
            _currentIndex = 0;
        }

        protected override void OnStop()
        {
        }
    }
}