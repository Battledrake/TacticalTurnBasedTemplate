using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BattleDrakeCreations.BehaviorTree
{
    public class BTNodeView : Node
    {
        public Action<BTNodeView> OnNodeSelected;

        public BTNode Node { get => _node; set => _node = value; }
        public Port InputPort { get => _inputPort; set => _inputPort = value; }
        public Port OutputPort { get => _outputPort; set => _outputPort = value; }

        [HideInInspector][SerializeField] private BTNode _node;
        [HideInInspector][SerializeField] private Port _inputPort;
        [HideInInspector][SerializeField] private Port _outputPort;
        private Label _descriptionLabel;
        private float _visualDisplayTime = 1f;

        public BTNodeView(BTNode node, VisualTreeAsset nodeXml) : base(AssetDatabase.GetAssetPath(nodeXml))
        {
            _node = node;
            this.title = node.title;
            this.viewDataKey = node.Guid;
            _descriptionLabel = this.Q<Label>("description");
            _descriptionLabel.text = _node.description;

            style.left = node.Position.x;
            style.top = node.Position.y;

            if (node.GetType() != typeof(RootNode))
                CreateInputPort();

            CreateOutputPort();

            if (node.GetType() == typeof(RootNode))
            {
                this.capabilities &= ~Capabilities.Deletable;
            }

            SetupClasses();
        }

        private void SetupClasses()
        {
            switch (_node)
            {
                case TaskNode:
                    AddToClassList("task");
                    break;
                case CompositeNode:
                    AddToClassList("composite");
                    break;
                case DecoratorNode:
                    AddToClassList("decorator");
                    break;
                case RootNode:
                    AddToClassList("root");
                    break;
            }
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return BTNodePort.Create<Edge>(orientation, direction, capacity, type);
        }

        private void CreateInputPort()
        {
            _inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));

            if (_inputPort != null)
            {
                _inputPort.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(_inputPort);
            }
        }

        private void CreateOutputPort()
        {
            switch (_node)
            {
                case CompositeNode:
                    _outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                case DecoratorNode:
                case RootNode:
                    _outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
            }

            if (_outputPort != null)
            {
                _outputPort.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(_outputPort);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(_node, "Behavior Tree (Set Position");
            _node.Position = new(newPos.xMin, newPos.yMin);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            OnNodeSelected?.Invoke(this);
        }

        public void SortChildren()
        {
            CompositeNode composite = _node as CompositeNode;
            if (composite)
            {
                composite.Children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(BTNode left, BTNode right)
        {
            return left.Position.x < right.Position.x ? -1 : left.Position.x == right.Position.x ? 0 : 1;
        }

        public void UpdateVisual()
        {
            _descriptionLabel.text = _node.description;

            //Removing the classes lets us hide the evaluation result border.
            RemoveFromClassList("succeeded");
            RemoveFromClassList("failed");
            RemoveFromClassList("running");
            if (Application.isPlaying)
            {
                switch (_node.Result)
                {
                    case NodeResult.Succeeded:
                        if (Time.time < _node.LastEvaluationTime + _visualDisplayTime)
                                AddToClassList("succeeded");
                        break;
                    case NodeResult.Failed:
                        if (Time.time < _node.LastEvaluationTime + _visualDisplayTime)
                            AddToClassList("failed");
                        break;
                    case NodeResult.Running:
                        if (_node.HasStarted && Time.time < _node.LastEvaluationTime + _visualDisplayTime)
                            AddToClassList("running");
                        break;
                }
            }
        }
    }
}
