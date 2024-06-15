using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public enum NodeResult
    {
        Succeeded,
        Failed,
        Running
    }

    public abstract class BTNode : ScriptableObject
    {
        public virtual string title { get => "BTNode"; }
        public virtual string description { get => ""; }
        public string Guid { get => _guid; set => _guid = value; }
        public NodeResult Result { get => _result; set => _result = value; }
        public Vector2 Position { get => _position; set => _position = value; }
        public bool HasStarted { get => _hasStarted; set => _hasStarted = value; }
        public float LastEvaluationTime { get => _lastEvaluationTime; }

        //In order for undo functionality to work properly, the variables needed to be either public or marked with SerializeField and we don't want to modify these in the inspector.
        [HideInInspector][SerializeField] protected string _guid;
        [HideInInspector][SerializeField] protected Vector2 _position = new();

        protected NodeResult _result = NodeResult.Running;
        protected bool _hasStarted = false;
        private BehaviorTree _tree;
        protected Blackboard _blackboard;
        protected IBehaviorTreeAgent _agent;
        private float _lastEvaluationTime = 0f;

        public void Init(BehaviorTree behaviorTree, Blackboard blackboard, IBehaviorTreeAgent agent)
        {
            _tree = behaviorTree;
            _blackboard = blackboard;
            _agent = agent;
        }

        public NodeResult Evaluate()
        {
            if (!_hasStarted)
            {
                OnStart();
                _hasStarted = true;
            }

            _result = OnEvaluate();

            if (_result == NodeResult.Failed || _result == NodeResult.Succeeded)
            {
                OnStop();
                _hasStarted = false;
            }
            _lastEvaluationTime = Time.time;
            return _result;
        }

        public virtual BTNode Clone()
        {
            return Instantiate(this);
        }

        public void Abort()
        {
            _tree.Traverse(this, (node) =>
            {
                node.HasStarted = false;
                node.Result = NodeResult.Failed;
                node.OnStop();
            });
        }

        protected abstract void OnStart();
        protected abstract NodeResult OnEvaluate();
        protected abstract void OnStop();
    }
}
