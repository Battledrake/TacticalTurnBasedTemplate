using System.Collections.Generic;
using UnityEngine;
using BattleDrakeCreations.BehaviorTree;
using System.Linq;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate.BehaviorTree
{
    public class FindNearestEnemy : TaskNode
    {
        public override string title { get => "Find Nearest Enemy"; }
        public override string description { get => ""; }

        private BlackboardKey _enemyTeamKey;
        private BlackboardKey _targetUnitKey;
        private int _enemyTeamIndex = -1;

        protected override void OnStart()
        {
            _enemyTeamKey = _blackboard.GetOrRegisterKey("EnemyTeam");
            _targetUnitKey = _blackboard.GetOrRegisterKey("TargetUnit");
            if (_blackboard.TryGetValue(_enemyTeamKey, out int teamIndex))
            {
                _enemyTeamIndex = teamIndex;
            }
        }

        protected override NodeResult OnEvaluate()
        {
            if (_enemyTeamIndex < 0)
                _result = NodeResult.Failed;

            HashSet<Unit> playerUnits = CombatManager.Instance.UnitTeams[_enemyTeamIndex];
            if (playerUnits.Count == 0)
            {
                Debug.Log("No enemies");
                _result = NodeResult.Failed;
            }


            Unit closestUnit = playerUnits.Last();
            float shortestUnitDist = Mathf.Infinity;
            foreach (Unit unit in playerUnits)
            {
                float distance = Vector3.Distance(unit.transform.position, closestUnit.transform.position);
                if (distance < shortestUnitDist)
                {
                    closestUnit = unit;
                    shortestUnitDist = distance;
                }
            }
            _blackboard.SetValue(_targetUnitKey, closestUnit);
            _result = NodeResult.Succeeded;

            return _result;
        }

        protected override void OnStop()
        {
        }
    }
}