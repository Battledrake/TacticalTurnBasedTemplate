using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public interface IBehaviorTreeAgent
    {
        //This interface is added in node binding. Populate interface with needed Getters/Properties (GameObject, Transform, NavAgent, Etc...) for nodes to access outside. 
        //Separate from blackboard data.
        public Unit Unit { get; }
        public AbilitySystem AbilitySystem { get; }
        public TacticsGrid TacticsGrid { get; }
        public GridMovement GridMovement { get; }
    }
}