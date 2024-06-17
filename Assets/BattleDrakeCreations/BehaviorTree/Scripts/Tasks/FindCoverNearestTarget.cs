using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;
using BattleDrakeCreations.TacticalTurnBasedTemplate;

public class FindCoverNearestTarget : TaskNode
{
    [SerializeField] private float _searchRadius;

    public override string title { get => "Find Cover Nearest"; }

    private BlackboardKey _targetIndexKey;
    private BlackboardKey _targetUnitKey;
    protected override void OnStart()
    {
        _targetIndexKey = _blackboard.GetOrRegisterKey("TargetIndex");
        _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {

        if (!_blackboard.TryGetValue(_targetUnitKey, out Unit targetUnit))
            return NodeResult.Failed;

        GridIndex closestIndex = GridIndex.Invalid();
        float closestDistance = Mathf.Infinity;
        Vector3 targetUnitPosition = targetUnit.transform.position;

        foreach (KeyValuePair<GridIndex, Cover> coverPair in _agent.TacticsGrid.Covers)
        {
            Vector3 coverPosition = _agent.TacticsGrid.GetTilePositionFromIndex(coverPair.Key);
            float distanceFromAI = Vector3.Distance(_agent.Unit.transform.position, coverPosition);
            float distanceFromTarget = Vector3.Distance(targetUnitPosition, coverPosition);

            int actionPoints = _agent.AbilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints);

            if (distanceFromAI > (actionPoints > 1 ? _agent.Unit.MoveRange * 2 : _agent.Unit.MoveRange)) 
                continue;

            if (distanceFromTarget < closestDistance)
            {
                for (int i = 0; i < coverPair.Value.data.Count; i++)
                {
                    GridIndex direction = coverPair.Value.data[i].direction;
                    Vector3 difference = targetUnit.transform.position - coverPosition;
                    float dotProduct = direction.x * difference.x + direction.z * difference.z;
                    if(dotProduct > 0.25f)
                    {
                        closestDistance = distanceFromAI;
                        closestIndex = coverPair.Key;
                        break;
                    }
                }
            }
        }
        if (closestIndex != GridIndex.Invalid())
        {
            _blackboard.SetValue(_targetIndexKey, closestIndex);
            return NodeResult.Succeeded;
        }
        else
        {
            return NodeResult.Failed;
        }
    }
}
