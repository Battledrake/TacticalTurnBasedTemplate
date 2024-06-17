using BattleDrakeCreations.BehaviorTree;
using System.Collections.Generic;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
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
        }

        protected override void OnStop()
        {
        }

        protected override NodeResult OnEvaluate()
        {
            if (_isMoving)
                return NodeResult.Running;

            if (_hasArrived)
            {
                _hasArrived = false;
                return NodeResult.Succeeded;
            }

            if (!_blackboard.TryGetValue(_targetIndexKey, out GridIndex targetIndex))
                return NodeResult.Failed;

            _targetIndex = targetIndex;

            if (_targetIndex == _agent.Unit.GridIndex)
                return NodeResult.Succeeded;

            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_agent.Unit);
            PathfindingResult pathResult = _agent.TacticsGrid.Pathfinder.FindPath(_agent.Unit.GridIndex, _targetIndex, pathParams);
            if (pathResult.Result != PathResult.SearchFail)
            {
                int maxTravel = _agent.AbilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints) >= 2 ? _agent.Unit.MoveRange * 2 : _agent.Unit.MoveRange;
                List<GridIndex> pathIndexes = new();
                float pathLength = 0f;
                for (int i = 0; i < pathResult.Path.Count; i++)
                {
                    if (pathResult.Path[i].traversalCost > maxTravel)
                    {
                        break;
                    }
                    else
                    {
                        pathLength = pathResult.Path[i].traversalCost;
                        pathIndexes.Add(pathResult.Path[i].index);
                    }

                }
                _agent.Unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                CombatManager.Instance.MoveUnit(_agent.Unit, pathIndexes, pathLength);
                _isMoving = true;
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
}