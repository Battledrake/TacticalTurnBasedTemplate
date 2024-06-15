using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;
using System;

public class FindPatrolPoint : TaskNode
{
    [SerializeField] private string _stringKey = "PatrolPoint";
    [SerializeField] private bool _randomOrder;

    public override string title { get => "Find Patrol Point"; }
    public override string description { get => $"Point: {_currentPoint}, Position: {_pointPosition}"; }

    private Transform _patrolPointContainer;
    private Vector3 _pointPosition;
    private int _currentPoint = 0;
    private BlackboardKey _blackboardKey;

    protected override void OnStart()
    {
        _blackboardKey = _blackboard.GetOrRegisterKey(_stringKey);
        _patrolPointContainer = GameObject.Find("PatrolPoints").transform;
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {
        _result = NodeResult.Running;
        if (SetNextPatrolPoint())
            _result = NodeResult.Succeeded;

        return _result; ;
    }

    private bool SetNextPatrolPoint()
    {
        if (_randomOrder)
            _currentPoint = UnityEngine.Random.Range(0, _patrolPointContainer.childCount);
        else
            _currentPoint = _currentPoint % _patrolPointContainer.childCount;

        _pointPosition = _patrolPointContainer.GetChild(_currentPoint).position;
        _blackboard.SetValue(_blackboardKey, _pointPosition);
        _currentPoint++;
        return true;
    }
}
