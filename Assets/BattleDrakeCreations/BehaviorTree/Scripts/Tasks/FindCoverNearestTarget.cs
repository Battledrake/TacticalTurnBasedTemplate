using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;
using BattleDrakeCreations.TacticalTurnBasedTemplate;

public class FindCoverNearestTarget : TaskNode
{
    [SerializeField] private float _coverDirDotTolerance = 0.25f;
    public override string title { get => "Find Cover Nearest Target"; }

    private BlackboardKey _targetIndexKey;
    private BlackboardKey _targetUnitKey;
    private BlackboardKey _activeAbilityKey;
    protected override void OnStart()
    {
        _targetIndexKey = _blackboard.GetOrRegisterKey("TargetIndex");
        _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");
        _activeAbilityKey = _blackboard.GetOrRegisterKey("ActiveAbility");
    }

    protected override void OnStop()
    {
    }

    protected override NodeResult OnEvaluate()
    {

        if (!_blackboard.TryGetValue(_targetUnitKey, out Unit targetUnit))
            return NodeResult.Failed;

        if (!_blackboard.TryGetValue(_activeAbilityKey, out Ability ability))
            return NodeResult.Failed;

        _agent.TacticsGrid.GetTileDataFromIndex(targetUnit.GridIndex, out TileData targetTile);

        GridIndex closestIndex = GridIndex.Invalid();
        float closestDistance = Mathf.Infinity;
        Vector3 targetUnitPosition = targetUnit.transform.position;

        int actionPoints = _agent.AbilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints);
        int maxTravelDistance = actionPoints > 1 ? _agent.Unit.MoveRange * 2 : _agent.Unit.MoveRange;

        foreach (KeyValuePair<GridIndex, Cover> coverPair in _agent.TacticsGrid.Covers)
        {
            _agent.TacticsGrid.GetTileDataFromIndex(coverPair.Key, out TileData coverTile);

            Vector3 coverPosition = coverTile.tileMatrix.GetPosition();
            float distanceFromAI = PathfindingStatics.GetDiagonalDistance(_agent.Unit.GridIndex, coverPair.Key);
            float distanceFromTarget = PathfindingStatics.GetDiagonalDistance(targetUnit.GridIndex, coverPair.Key);
            Debug.Log($"DistanceFromAI: {distanceFromAI}, DistanceFromTarget: {distanceFromTarget}");

            if (distanceFromAI > maxTravelDistance) 
                continue;

            if (!CombatManager.Instance.GetAbilityRange(coverPair.Key, ability.RangeData).Contains(targetUnit.GridIndex))
                continue;

            if (!AbilityStatics.HasLineOfSight(coverTile, targetTile, ability.RangeData.lineOfSightData.height, ability.RangeData.lineOfSightData.offsetDistance))
                continue;

            if (distanceFromTarget < closestDistance)
            {
                for (int i = 0; i < coverPair.Value.data.Count; i++)
                {
                    GridIndex direction = coverPair.Value.data[i].direction;
                    Vector3 difference = targetUnit.transform.position - coverPosition;
                    difference.Normalize();
                    float dotProduct = direction.x * difference.x + direction.z * difference.z;
                    if(dotProduct > _coverDirDotTolerance)
                    {
                        Debug.Log(dotProduct);
                        closestDistance = distanceFromTarget;
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
