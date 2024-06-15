using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public abstract class CompositeNode : BTNode
    {
        public override string title { get => "Composite"; }

        public List<BTNode> Children { get => _children; }

        [HideInInspector]
        [SerializeField] protected List<BTNode> _children = new List<BTNode>();

        public void AddChild(BTNode childToAdd)
        {
            _children.Add(childToAdd);
        }

        public bool RemoveChild(BTNode childToRemove)
        {
            return _children.Remove(childToRemove);
        }

        public override BTNode Clone()
        {
            CompositeNode node = Instantiate(this);
            node._children = node._children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}