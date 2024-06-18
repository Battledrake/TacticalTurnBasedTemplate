using System;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{

    [Serializable]
    public abstract class DecoratorNode : BTNode
    {
        public enum IsSet
        {
            IsSet,
            IsNotSet
        }
        public override string title { get => "Decorator"; }

        public BTNode Child { get => _child; set => _child = value; }

        [HideInInspector][SerializeField] protected BTNode _child;

        public override BTNode Clone()
        {
            DecoratorNode node = Instantiate(this);
            node._child = node._child.Clone();
            return node;
        }
    }
}
