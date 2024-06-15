using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;

public class MoveDirectlyTo : TaskNode
{
    [SerializeField] private string _stringKey = "PatrolPoint";
    [SerializeField] private float _arrivalRange = 0.2f;
    [SerializeField] private float _agentSpeed = 5f;

    public override string title { get => "Move Directly To"; }
    public override string description { get => $"Target: {_targetPosition}"; }

    private Vector3 _targetPosition;
    private BlackboardKey _blackboardKey;
    private bool _hasArrived;
    private Transform _agentTransform;

    protected override void OnStart()
    {
        _hasArrived = false;
        _blackboardKey = _blackboard.GetOrRegisterKey(_stringKey);
        if (_blackboard.TryGetValue(_blackboardKey, out Vector3 target))
        {
            _targetPosition = target;
        }
        _agentTransform = _agent.Unit.transform;
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {
        _agentTransform.position = Vector3.MoveTowards(_agentTransform.position, _targetPosition, Time.deltaTime * _agentSpeed);
        if (Vector3.Distance(_agentTransform.position, _targetPosition) <= _arrivalRange)
        {
            _hasArrived = true;
        }

        if (_hasArrived)
            return NodeResult.Succeeded;
        else
            return NodeResult.Running;
    }
}
