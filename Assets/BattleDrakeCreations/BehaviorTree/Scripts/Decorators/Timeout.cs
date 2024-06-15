using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

public class Timeout : DecoratorNode
{
    [SerializeField] private float _timeUntilCancel = 2f;

    public override string title { get => "Timeout"; }
    public override string description { get => $"Timeout: {_timeUntilCancel:F2}, Remaining: {Mathf.Clamp((_startTime + _timeUntilCancel) - Time.time, 0, _startTime + _timeUntilCancel):F2}"; }

    private float _startTime;

    protected override void OnStart()
    {
        _startTime = Time.time;
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {
        _result = _child.Evaluate();
        if (Time.time >= _startTime + _timeUntilCancel)
            _result = NodeResult.Failed;

        return _result;
    }
}
