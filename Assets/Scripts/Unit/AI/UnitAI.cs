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
        StartTurn,
        DecideAbility,
        DecidePosition,
        MoveToPosition,
        UseAbility,
        EndTurn
    }

    [Serializable]
    public struct AIStateVisualLinker
    {
        public AIState aiState;
        public GameObject visualPrefab;
    }

    public static class EnumExtensions
    {
        public static TEnum Next<TEnum>(this TEnum value) where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            var currentIndex = values.IndexOf(value);
            var nextIndex = (currentIndex + 1) % values.Count;
            return values[nextIndex];
        }
    }

    public class UnitAI : MonoBehaviour
    {
        public event Action<AIState> OnAIStateChanged;

        [SerializeField] private float _delayBetweenStates = 0.5f;
        [SerializeField] private Transform _aiStateVisualContainer;
        [SerializeField] private List<AIStateVisualLinker> _aiStateVisualLinkers;

        //Stuff
        private AIState _currentState = AIState.None;
        private Dictionary<AIState, GameObject> _aiStateVisuals = new Dictionary<AIState, GameObject>();
        private Dictionary<AIState, Action> _aiActions = new Dictionary<AIState, Action>();
        private Action OnNextAIAction;
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
            _gridMovement = _unit.GetGridMovement();
            _tacticsGrid = _unit.GetTacticsGrid();
            _abilitySystem = this.GetComponentInParent<IAbilitySystem>().GetAbilitySystem();

            OnNextAIAction = AdvanceToNextState;

            for(int i = 0; i < _aiStateVisualLinkers.Count; i++)
            {
                _aiStateVisuals.TryAdd(_aiStateVisualLinkers[i].aiState, _aiStateVisualLinkers[i].visualPrefab);
            }

            _aiActions[AIState.StartTurn] = AdvanceToNextState;
            _aiActions[AIState.DecideAbility] = DecideAbility;
            _aiActions[AIState.DecidePosition] = DecidePosition;
            _aiActions[AIState.MoveToPosition] = MoveToPosition;
            _aiActions[AIState.UseAbility] = UseAbility;
            _aiActions[AIState.EndTurn] = AdvanceToNextState;

            CombatManager.Instance.OnCombatFinishing += CombatManager_OnCombatFinishing;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
        }

        private void CombatManager_OnCombatEnded()
        {
            ClearAIVisuals();
        }

        private void CombatManager_OnCombatFinishing(int winTeam)
        {
            _currentState = AIState.EndTurn;
        }

        private void DecideAbility()
        {
            if(_activeAbility == null)
            {
                List<Ability> abilities = _abilitySystem.GetAbilities();
                do
                {
                    _activeAbility = abilities[UnityEngine.Random.Range(0, abilities.Count)];
                } while (_activeAbility.AffectsFriendly || _activeAbility.GetActiveCooldown() > 0 || _activeAbility.GetUsesLeft() == 0);
            }
            AdvanceToNextState();
        }

        private void DecidePosition()
        {
            if(_targetUnit == null)
            {
                HashSet<Unit> playerUnits = CombatManager.Instance.UnitTeams[0];
                if (playerUnits.Count == 0)
                {
                    Debug.Log("No enemies");
                    AdvanceToNextState();
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

            if(_targetUnit != null)
            {
                List<GridIndex> abilityRangeIndexes = CombatManager.Instance.GetAbilityRange(_targetUnit.GetGridIndex(), _activeAbility.GetRangeData());
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
            AdvanceToNextState();
        }

        private void MoveToPosition()
        {
            if (_unit.GetGridIndex() != _targetIndex)
            {
                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(_unit, _unit.GetMoveRange() * 2, true);
                PathfindingResult pathResult = _tacticsGrid.GridPathfinder.FindPath(_unit.GetGridIndex(), _targetIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    if (pathResult.Path.Count > 0)
                    {
                        if (pathResult.Length > _unit.GetMoveRange())
                        {
                            _currentState = AIState.UseAbility;
                        }
                        _unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                        CombatManager.Instance.MoveUnit(_unit, pathResult.Path, pathResult.Length);
                    }
                    else
                    {
                        _currentState = AIState.StartTurn;
                        _activeAbility = null;
                        _targetUnit = null;
                        _targetIndex = GridIndex.Invalid();
                        AdvanceToNextState();
                    }
                }
            }
            else
            {
                AdvanceToNextState();
            }
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            _unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;
            AdvanceToNextState();
        }

        private void UseAbility()
        {
            GridIndex targetIndex = _activeAbility.GetRangeData().rangeMinMax.y == 0 ? _unit.GetGridIndex() : _targetUnit.GetGridIndex();
            if(CombatManager.Instance.GetAbilityRange(_unit.GetGridIndex(), _activeAbility.GetRangeData()).Contains(targetIndex))
            {
                _activeAbility.OnAbilityEnded += Ability_OnAbilityEnded;
                if(!CombatManager.Instance.TryActivateAbility(_activeAbility, _unit.GetGridIndex(), targetIndex))
                {
                    _activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;
                    AdvanceToNextState();
                }
            }
            else
            {
                Debug.Log("Unit not in range");
                AdvanceToNextState();
            }
        }

        private void Ability_OnAbilityEnded(Ability ability)
        {
            _activeAbility.OnAbilityEnded -= Ability_OnAbilityEnded;
            _activeAbility = null;
            _targetUnit = null;
            _targetIndex = GridIndex.Invalid();
            AdvanceToNextState();
        }

        private void AdvanceToNextState()
        {
            if (_currentState == AIState.EndTurn)
            {
                _currentState = AIState.None;
                _unit.AIEndTurn();
            }
            else
            {
                StartCoroutine(SetAIState(_currentState.Next()));
            }
        }

        private void ClearAIVisuals()
        {

            for (int i = 0; i < _aiStateVisualContainer.childCount; i++)
            {
                Destroy(_aiStateVisualContainer.GetChild(i).gameObject);
            }
        }

        public void RunAILogic()
        {
            ClearAIVisuals();

            StartCoroutine(SetAIState(AIState.StartTurn));
        }

        private IEnumerator SetAIState(AIState newState)
        {
            _currentState = newState;
            OnAIStateChanged?.Invoke(_currentState);
            Instantiate(_aiStateVisuals[newState], _aiStateVisualContainer);
            yield return new WaitForSeconds(_delayBetweenStates);
            _aiActions[newState]();
        }
    }
}