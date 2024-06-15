using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BattleDrakeCreations.BehaviorTree
{
    [CreateAssetMenu(fileName = "New Behavior Tree", menuName = "Behavior Tree/New Behavior Tree")]
    public class BehaviorTree : ScriptableObject
    {
        [SerializeField] private BlackboardData _blackboardData;

        public Blackboard Blackboard { get => _blackboard; }
        public BTNode RootNode { get => _rootNode; set => _rootNode = value; }
        public List<BTNode> Nodes { get => _nodes; }

        [HideInInspector][SerializeField] private BTNode _rootNode;
        [HideInInspector][SerializeField] List<BTNode> _nodes = new();
        private Blackboard _blackboard = new();

        public NodeResult ExecuteTree()
        {
            return _rootNode.Evaluate();
        }

        public List<BTNode> GetChildren(BTNode parent)
        {
            List<BTNode> children = new List<BTNode>();

            RootNode rootNode = parent as RootNode;
            if (rootNode != null && rootNode.ChildNode != null)
            {
                children.Add(rootNode.ChildNode);
            }

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator && decorator.Child != null)
            {
                children.Add(decorator.Child);
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite)
            {
                return composite.Children;
            }

            return children;
        }

        public void Traverse(BTNode node, Action<BTNode> visitor)
        {
            if (node)
            {
                visitor.Invoke(node);
                List<BTNode> children = GetChildren(node);
                children.ForEach((n) => Traverse(n, visitor));
            }
        }

        public void Bind(IBehaviorTreeAgent agent)
        {
            _blackboardData.SetValuesOnBlackboard(_blackboard);

            Traverse(_rootNode, node =>
            {
                node.Init(this, _blackboard, agent);
            });
        }

        public BehaviorTree Clone()
        {
            BehaviorTree tree = Instantiate(this);
            tree._rootNode = tree._rootNode.Clone();
            tree._nodes = new List<BTNode>();
            Traverse(tree._rootNode, (n) =>
            {
                tree.Nodes.Add(n);
            });
            return tree;
        }

#if UNITY_EDITOR

        public BTNode CreateNode<T>(Vector2 nodePosition)
        {
            return CreateNode(typeof(T), nodePosition);
        }

        public BTNode CreateNode(Type type, Vector2 nodePosition)
        {
            BTNode newNode = CreateInstance(type) as BTNode;
            newNode.name = type.Name;
            newNode.Guid = GUID.Generate().ToString();
            newNode.Position = nodePosition;

            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(newNode, this);
            Undo.RegisterCreatedObjectUndo(newNode, "Behavior Tree (CreateNode)");

            Undo.RecordObject(this, "Behavior Tree (CreateAsset)");
            _nodes.Add(newNode);

            AssetDatabase.SaveAssets();

            return newNode;
        }

        public void DeleteNode(BTNode node)
        {
            Undo.RecordObject(this, "Behavior Tree (DeleteNode)");
            _nodes.Remove(node);

            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(BTNode parent, BTNode child)
        {
            RootNode rootNode = parent as RootNode;
            if (rootNode != null)
            {
                Undo.RecordObject(rootNode, "Behavior Tree (AddChild)");
                rootNode.ChildNode = child;
            }

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator != null)
            {
                Undo.RecordObject(decorator, "Behavior Tree (AddChild)");
                decorator.Child = child;
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite != null)
            {
                Undo.RecordObject(composite, "Behavior Tree (AddChild)");
                composite.AddChild(child);
            }
        }

        public void RemoveChild(BTNode parent, BTNode child)
        {
            RootNode rootNode = parent as RootNode;
            if (rootNode != null)
            {
                Undo.RecordObject(rootNode, "Behavior Tree (RemoveChild");
                rootNode.ChildNode = null;
            }

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator != null)
            {
                Undo.RecordObject(decorator, "Behavior Tree (RemoveChild");
                decorator.Child = null;
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite != null)
            {
                Undo.RecordObject(composite, "Behavior Tree (RemoveChild");
                composite.RemoveChild(child);
            }
        }
#endif
    }
}
