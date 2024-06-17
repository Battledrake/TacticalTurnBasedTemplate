using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

public class AlwaysSucceed : DecoratorNode
{
    public override string title { get => "Always Succeed"; }
    public override string description { get => "Returns success"; }

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {
        _child.Evaluate();

        return NodeResult.Succeeded;
    }
}
