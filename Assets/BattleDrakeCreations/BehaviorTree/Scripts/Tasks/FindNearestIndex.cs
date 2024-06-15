using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;
using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Linq;

public class FindNearestIndex : TaskNode
{
    public override string title { get => "Find Nearest Index for Ability"; }

    private BlackboardKey _targetUnitKey;
    private BlackboardKey _activeAbilityKey;
    private BlackboardKey _targetIndexKey;
    protected override void OnStart() 
    {
        _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");
        _activeAbilityKey = _blackboard.GetOrRegisterKey("ActiveAbility");
        _targetIndexKey = _blackboard.GetOrRegisterKey("TargetIndex");
    }

    protected override void OnStop() 
    {
    }

    protected override NodeResult OnEvaluate()
    {
        _blackboard.TryGetValue(_targetUnitKey, out Unit targetUnit);
        _blackboard.TryGetValue(_activeAbilityKey, out Ability activeAbility);

        if (targetUnit == null || activeAbility == null)
            return NodeResult.Failed;

        List<GridIndex> abilityRangeIndexes = CombatManager.Instance.GetAbilityRange(targetUnit.GridIndex, activeAbility.RangeData);
        if (abilityRangeIndexes.Count == 0)
            return NodeResult.Failed;

        GridIndex closestIndex = abilityRangeIndexes.Last();
        float shortestAbilityIndexDist = Mathf.Infinity;
        for (int i = 0; i < abilityRangeIndexes.Count; i++)
        {
            float distance = Vector3.Distance(_agent.Unit.transform.position, _agent.TacticsGrid.GetTilePositionFromIndex(abilityRangeIndexes[i]));
            if (distance < shortestAbilityIndexDist)
            {
                closestIndex = abilityRangeIndexes[i];
                shortestAbilityIndexDist = distance;
            }
        }
        _blackboard.SetValue(_targetIndexKey, closestIndex);

        return NodeResult.Succeeded;
    }
}
