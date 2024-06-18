using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

public class WaitWhileMoving : TaskNode
{

    public override string title { get => "Wait While Moving"; }
    public override string description { get => $"Is Moving: {_agent?.GridMovement?.IsMoving}"; }
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {
        if (_agent.GridMovement.IsMoving)
        {
            return NodeResult.Running;
        }
        else
        {
            return NodeResult.Succeeded;
        }
    }
}
