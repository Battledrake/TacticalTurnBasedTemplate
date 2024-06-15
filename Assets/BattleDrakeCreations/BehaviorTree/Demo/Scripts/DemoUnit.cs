using BattleDrakeCreations.BehaviorTree;
using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoUnit : MonoBehaviour, IBehaviorTreeAgent
{
    public Unit Unit => throw new System.NotImplementedException();

    public AbilitySystem AbilitySystem => throw new System.NotImplementedException();

    public GridMovement GridMovement => throw new System.NotImplementedException();

    public TacticsGrid TacticsGrid => throw new System.NotImplementedException();
}
