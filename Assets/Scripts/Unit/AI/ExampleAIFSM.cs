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
        PickAbility,
        FindTargetUnit,
        FindPosition,
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

        private Ability _activeAbility = null;
        private Unit _targetUnit = null;
        private GridIndex _targetIndex = GridIndex.Invalid();

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
            _aiActions[AIState.PickAbility] = PickRandomAbility;
            _aiActions[AIState.FindTargetUnit] = FindTargetUnit;
            _aiActions[AIState.FindPosition] = FindPosition;
            _aiActions[AIState.MoveToPosition] = MoveToPosition;
            _aiActions[AIState.UseAbility] = UseAbility;
            _aiActions[AIState.EndingTurn] = EndingTurn;

            _unit.OnTurnEnded += Unit_OnTurnEnded;
            CombatManager.Instance.OnCombatFinishing += CombatManager_OnCombatFinishing;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
        }

        private void OnDisable()
        {
            _unit.OnTurnEnded -= Unit_OnTurnEnded;
            CombatManager.Instance.OnCombatFinishing -= CombatManager_OnCombatFinishing;
            CombatManager.Instance.OnCombatEnded -= CombatManager_OnCombatEnded;
        }

        private void Unit_OnTurnEnded()
        {
            MakeDecision(AIState.EndingTurn, true);
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
            MakeDecision(AIState.StartingTurn, true);
        }

        private void EndingTurn()
        {
            _currentState = AIState.None;
            OnEndTurn?.Invoke();
        }

        private bool UnitHasEnoughAbilityPoints(int amount)
        {
            if (_abilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints) >= amount)
                return true;
            else
                return false;
        }

        private void MakeDecision(AIState attemptedState, bool succeeded)
        {
            switch (attemptedState)
            {
                case AIState.None:
                    break;
                case AIState.StartingTurn:
                    {
                        if (_activeAbility && !_activeAbility.IsFriendlyOnly)
                        {
                            AdvanceToNextState(AIState.FindTargetUnit);
                        }
                        else
                        {
                            AdvanceToNextState(AIState.PickAbility);
                        }
                    }
                    break;
                case AIState.PickAbility:
                    {
                        if (succeeded)
                        {
                            if (_activeAbility.IsFriendlyOnly)
                            {
                                _targetUnit = _unit;
                                AdvanceToNextState(AIState.UseAbility);
                            }
                            else
                            {
                                if (_targetUnit)
                                    AdvanceToNextState(AIState.FindPosition);
                                else
                                    AdvanceToNextState(AIState.FindTargetUnit);
                            }
                        }
                        else
                        {
                            AdvanceToNextState(AIState.EndingTurn);
                        }
                    }
                    break;
                case AIState.FindTargetUnit:
                    {
                        if (succeeded)
                        {
                            AdvanceToNextState(AIState.FindPosition);
                        }
                        else
                        {
                            AdvanceToNextState(AIState.EndingTurn);
                        }
                    }
                    break;
                case AIState.FindPosition:
                    {
                        if (succeeded)
                        {
                            if (_targetIndex == _unit.GridIndex)
                                AdvanceToNextState(AIState.UseAbility);
                            else
                                AdvanceToNextState(AIState.MoveToPosition);
                        }
                        else
                        {
                            AdvanceToNextState(AIState.FindTargetUnit);
                        }
                    }
                    break;
                case AIState.MoveToPosition:
                    {
                        if (succeeded)
                        {
                            if (UnitHasEnoughAbilityPoints(_activeAbility.ActionPointCost))
                                AdvanceToNextState(AIState.UseAbility);
                            else
                                AdvanceToNextState(AIState.EndingTurn);
                        }
                        else
                        {
                            AdvanceToNextState(AIState.EndingTurn);
                        }
                    }
                    break;
                case AIState.UseAbility:
                    {
                        if (succeeded)
                        {
                            if (!_activeAbility.EndTurnOnUse)
                            {
                                if (_activeAbility.IsFriendlyOnly)
                                {
                                    _targetUnit = null;
                                    AdvanceToNextState(AIState.PickAbility);
                                }
                                else
                                {
                                    if (UnitHasEnoughAbilityPoints(_activeAbility.ActionPointCost))
                                    {
                                        AdvanceToNextState(AIState.UseAbility);
                                    }
                                    else
                                    {
                                        AdvanceToNextState(AIState.PickAbility);
                                    }
                                }
                            }
                            else
                            {
                                AdvanceToNextState(AIState.EndingTurn);
                            }
                        }
                        else
                        {
                            if (UnitHasEnoughAbilityPoints(_activeAbility.ActionPointCost))
                            {
                                AdvanceToNextState(AIState.MoveToPosition);
                            }
                            else
                            {
                                AdvanceToNextState(AIState.PickAbility);
                            }
                        }
                    }
                    break;
                case AIState.EndingTurn:
                    {
                        _currentState = AIState.None;
                    }
                    break;
            }
        }

        private void PickRandomAbility()
        {
            List<Ability> abilities = _abilitySystem.GetAbilities();
            int checkCount = 0;
            do
            {
                _activeAbility = abilities[UnityEngine.Random.Range(0, abilities.Count)];
                checkCount++;
            } while (
            (!UnitHasEnoughAbilityPoints(_activeAbility.ActionPointCost)
            || _activeAbility.ActiveCooldown > 0
            || _activeAbility.UsesLeft == 0)
            && checkCount < abilities.Count);

            if (checkCount >= abilities.Count)
            {
                MakeDecision(AIState.PickAbility, false);
            }

            MakeDecision(AIState.PickAbility, true);
        }

        private void FindTargetUnit()
        {
            HashSet<Unit> playerUnits = CombatManager.Instance.UnitTeams[0];
            if (playerUnits.Count == 0)
            {
                Debug.Log("No enemies");
                MakeDecision(AIState.FindTargetUnit, false);
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

            MakeDecision(AIState.FindTargetUnit, true);
        }

        private void FindPosition()
        {
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
            else
            {
                MakeDecision(AIState.FindPosition, false);
            }
            MakeDecision(AIState.FindPosition, true);
        }

        private void MoveToPosition()
        {
            PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_unit);
            PathfindingResult pathResult = _tacticsGrid.Pathfinder.FindPath(_unit.GridIndex, _targetIndex, pathParams);
            if (pathResult.Result != PathResult.SearchFail)
            {
                if (pathResult.Path.Count > 0)
                {
                    int maxTravel = UnitHasEnoughAbilityPoints(2) ? _unit.MoveRange * 2 : _unit.MoveRange;
                    List<GridIndex> pathIndexes = new();
                    float lastTraversalCost = 0f;
                    for (int i = 0; i < pathResult.Path.Count; i++)
                    {
                        if (pathResult.Path[i].traversalCost > maxTravel)
                        {
                            break;
                        }
                        else
                        {
                            lastTraversalCost = pathResult.Path[i].traversalCost;
                            pathIndexes.Add(pathResult.Path[i].index);
                        }
                    }
                    _unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                    CombatManager.Instance.MoveUnit(_unit, pathIndexes, lastTraversalCost);
                }
                else
                {
                    MakeDecision(AIState.MoveToPosition, false);
                }
            }
            else
            {
                MakeDecision(AIState.MoveToPosition, false);
            }

        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;

            MakeDecision(AIState.MoveToPosition, true);
        }

        private void UseAbility()
        {
            if (CombatManager.Instance.GetAbilityRange(_unit.GridIndex, _activeAbility.RangeData).Contains(_targetUnit.GridIndex))
            {
                _activeAbility.OnAbilityEnded += Ability_OnAbilityEnded;
                if (!CombatManager.Instance.UseAbility(_activeAbility, _unit.GridIndex, _targetUnit.GridIndex))
                {
                    _activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;
                    MakeDecision(AIState.UseAbility, false);
                }
            }
            else
            {
                MakeDecision(AIState.UseAbility, false);
            }
        }

        private void Ability_OnAbilityEnded(Ability ability)
        {
            _activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;

            MakeDecision(AIState.UseAbility, true);
        }

        private void AdvanceToNextState(AIState stateToMoveTo)
        {
            if (_currentState == AIState.None)
                return;
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