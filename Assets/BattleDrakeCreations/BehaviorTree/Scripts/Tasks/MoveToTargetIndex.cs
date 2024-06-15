using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;
using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;

public class MoveToTargetIndex : TaskNode
{
    public override string title { get => "Move To Target Index"; }
    public override string description { get => $"Dest: {_targetIndex}"; }

    private BlackboardKey _targetIndexKey;
    private GridIndex _targetIndex;

    private bool _isMoving = false;
    private bool _hasArrived = false;

    protected override void OnStart()
    {
        _targetIndexKey = _blackboard.GetOrRegisterKey("TargetIndex");
        _isMoving = false;
        _hasArrived = false;
        _result = NodeResult.Running;
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {
        if (_isMoving)
            return NodeResult.Running;

        if (_hasArrived)
            return NodeResult.Succeeded;

        if (!_blackboard.TryGetValue(_targetIndexKey, out GridIndex targetIndex))
            return NodeResult.Failed;

        _targetIndex = targetIndex;

        if (_targetIndex == _agent.Unit.GridIndex)
            return NodeResult.Succeeded;

        PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_agent.Unit, _agent.Unit.MoveRange * 2, true);
        PathfindingResult pathResult = _agent.TacticsGrid.Pathfinder.FindPath(_agent.Unit.GridIndex, _targetIndex, pathParams);
        if (pathResult.Result != PathResult.SearchFail)
        {
            if (pathResult.Path.Count > 0)
            {
                _agent.Unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                CombatManager.Instance.MoveUnit(_agent.Unit, pathResult.Path, pathResult.Length);
                _isMoving = true;
            }
        }
        else
        {
            return NodeResult.Failed;
        }

        return NodeResult.Running;
    }

    private void Unit_OnUnitReachedDestination(Unit unit)
    {
        _agent.Unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;
        _isMoving = false;
        _hasArrived = true;
    }
}
