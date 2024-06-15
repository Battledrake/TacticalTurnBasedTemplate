using BattleDrakeCreations.BehaviorTree;
using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AIState
    {
        None,
        StartingTurn,
        DecideAbility,
        DecidePosition,
        MoveToPosition,
        UseAbility,
        EndingTurn
    }

    [Serializable]
    public struct AIStateVisualLinker
    {
        public AIState aiState;
        public GameObject visualPrefab;
    }

    public class ExampleAIFSM : MonoBehaviour
    {
        public event Action<AIState> OnAIStateChanged;
        public event Action OnEndTurn;

        [SerializeField] private float _delayBetweenStates = 0.5f;
        [SerializeField] private Transform _aiStateVisualContainer;
        [SerializeField] private List<AIStateVisualLinker> _aiStateVisualLinkers;

        //Stuff
        private AIState _currentState = AIState.None;
        private Dictionary<AIState, GameObject> _aiStateVisuals = new Dictionary<AIState, GameObject>();
        private Dictionary<AIState, Action> _aiActions = new Dictionary<AIState, Action>();

        //Can be pulled from CombatManager and selected based on conditions
        private List<Unit> _enemyUnits;

        private Ability _activeAbility;
        private Unit _targetUnit;
        private GridIndex _targetIndex;

        //Components
        private Unit _unit;
        private GridMovement _gridMovement;
        private AbilitySystem _abilitySystem;

        //Dependencies
        private TacticsGrid _tacticsGrid;


        private void Start()
        {
            _unit = this.GetComponentInParent<Unit>();
            _gridMovement = _unit.GridMovement;
            _tacticsGrid = _unit.TacticsGrid;
            _abilitySystem = this.GetComponentInParent<IAbilitySystem>().AbilitySystem;

            for (int i = 0; i < _aiStateVisualLinkers.Count; i++)
            {
                _aiStateVisuals.TryAdd(_aiStateVisualLinkers[i].aiState, _aiStateVisualLinkers[i].visualPrefab);
            }

            _aiActions[AIState.StartingTurn] = StartingTurn;
            _aiActions[AIState.DecideAbility] = DecideAbility;
            _aiActions[AIState.DecidePosition] = DecidePosition;
            _aiActions[AIState.MoveToPosition] = MoveToPosition;
            _aiActions[AIState.UseAbility] = UseAbility;
            _aiActions[AIState.EndingTurn] = EndingTurn;

            CombatManager.Instance.OnCombatFinishing += CombatManager_OnCombatFinishing;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
        }

        private void CombatManager_OnCombatEnded()
        {
            ClearAIVisuals();
        }

        private void CombatManager_OnCombatFinishing(int winTeam)
        {
            _currentState = AIState.EndingTurn;
        }

        private void StartingTurn()
        {
            ClearAIVisuals();
            AdvanceToNextState(AIState.DecideAbility);
        }

        private void EndingTurn()
        {
            _currentState = AIState.None;
            OnEndTurn?.Invoke();
        }

        private void DecideAbility()
        {
            if (_activeAbility == null)
            {
                List<Ability> abilities = _abilitySystem.GetAbilities();
                int checkCount = 0;
                do
                {
                    _activeAbility = abilities[UnityEngine.Random.Range(0, abilities.Count)];
                    checkCount++;
                } while (
                (_activeAbility.IsFriendlyOnly
                || _activeAbility.ActiveCooldown > 0
                || _activeAbility.UsesLeft == 0
                || _activeAbility.ActionPointCost > _abilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints))
                && checkCount < abilities.Count);

                if (checkCount >= abilities.Count)
                {
                    AdvanceToNextState(AIState.EndingTurn);
                }
            }
            AdvanceToNextState(AIState.DecidePosition);
        }

        private void DecidePosition()
        {
            if (_targetUnit == null)
            {
                HashSet<Unit> playerUnits = CombatManager.Instance.UnitTeams[0];
                if (playerUnits.Count == 0)
                {
                    Debug.Log("No enemies");
                    AdvanceToNextState(AIState.EndingTurn);
                    return;
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
                _targetUnit = closestUnit;
            }

            if (_targetUnit != null)
            {
                List<GridIndex> abilityRangeIndexes = CombatManager.Instance.GetAbilityRange(_targetUnit.GridIndex, _activeAbility.RangeData);
                GridIndex closestIndex = abilityRangeIndexes.Last();
                float shortestAbilityIndexDist = Mathf.Infinity;
                for (int i = 0; i < abilityRangeIndexes.Count; i++)
                {
                    float distance = Vector3.Distance(_unit.transform.position, _tacticsGrid.GetTilePositionFromIndex(abilityRangeIndexes[i]));
                    if (distance < shortestAbilityIndexDist)
                    {
                        closestIndex = abilityRangeIndexes[i];
                        shortestAbilityIndexDist = distance;
                    }
                }
                _targetIndex = closestIndex;
            }
            AdvanceToNextState(AIState.MoveToPosition);
        }

        private bool UnitHasEnoughAbilityPoints(int amount)
        {
            if (_abilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints) >= amount)
                return true;
            else
                return false;
        }

        private void MoveToPosition()
        {
            if (_unit.GridIndex != _targetIndex)
            {
                if (!UnitHasEnoughAbilityPoints(1))
                {
                    AdvanceToNextState(AIState.EndingTurn);
                    return;
                }

                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_unit);
                PathfindingResult pathResult = _tacticsGrid.Pathfinder.FindPath(_unit.GridIndex, _targetIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    int maxTravel = UnitHasEnoughAbilityPoints(2) ? _unit.MoveRange * 2 : _unit.MoveRange;
                    List<GridIndex> pathIndexes = new();
                    for (int i = 0; i < pathResult.Path.Count; i++)
                    {
                        if (pathResult.Path[i].traversalCost > maxTravel)
                            break;
                        else
                            pathIndexes.Add(pathResult.Path[i].index);
                    }
                    _unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                    CombatManager.Instance.MoveUnit(_unit, pathIndexes, maxTravel);
                }
                else
                {
                    Debug.LogWarning("Path Failed. Invalid Origin or TargetIndex. Ending Turn.");
                    AdvanceToNextState(AIState.EndingTurn);
                }
            }
            else
            {
                AdvanceToNextState(AIState.UseAbility);
            }
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;

            AdvanceToNextState(AIState.UseAbility);
        }

        private void UseAbility()
        {
            GridIndex targetIndex = _activeAbility.RangeData.rangeMinMax.y == 0 ? _unit.GridIndex : _targetUnit.GridIndex;
            if (CombatManager.Instance.GetAbilityRange(_unit.GridIndex, _activeAbility.RangeData).Contains(targetIndex))
            {
                _activeAbility.OnAbilityEnded += Ability_OnAbilityEnded;
                if (!CombatManager.Instance.TryActivateAbility(_activeAbility, _unit.GridIndex, targetIndex))
                {
                    _activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;
                    AdvanceToNextState(AIState.EndingTurn);
                }
            }
            else
            {
                Debug.Log("Unit not in range");
                AdvanceToNextState(AIState.MoveToPosition);
            }
        }

        private void Ability_OnAbilityEnded(Ability ability)
        {
            _activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;
            _activeAbility = null;
            _targetUnit = null;
            _targetIndex = GridIndex.Invalid();

            if (ability.EndTurnOnUse)
                AdvanceToNextState(AIState.EndingTurn);
            else
                AdvanceToNextState(AIState.DecideAbility);
        }

        private void AdvanceToNextState(AIState stateToMoveTo)
        {
            StartCoroutine(SetAIState(stateToMoveTo));
        }

        private void ClearAIVisuals()
        {
            for (int i = 0; i < _aiStateVisualContainer.childCount; i++)
            {
                Destroy(_aiStateVisualContainer.GetChild(i).gameObject);
            }
        }

        public IEnumerator SetAIState(AIState newState)
        {
            _currentState = newState;
            OnAIStateChanged?.Invoke(_currentState);
            yield return new WaitForSeconds(_delayBetweenStates);
            _aiActions[newState]();
            Instantiate(_aiStateVisuals[newState], _aiStateVisualContainer);
        }
    }
}