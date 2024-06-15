using BattleDrakeCreations.BehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AILogic
    {
        FSM,
        BehaviorTree
    }

    public class UnitAI : MonoBehaviour, IBehaviorTreeAgent
    {
        [SerializeField] private AILogic _aiLogic;

        //IBehaviorTreeAgent
        public Unit Unit => _unit;
        public AbilitySystem AbilitySystem => _abilitySystem;
        public TacticsGrid TacticsGrid => _tacticsGrid;
        public GridMovement GridMovement => _gridMovement;

        //Components
        private Unit _unit;
        private GridMovement _gridMovement;
        private AbilitySystem _abilitySystem;
        private BehaviorTreeRunner _btRunner;
        private ExampleAIFSM _exampleFSM;

        //Dependencies
        private TacticsGrid _tacticsGrid;


        private void Awake()
        {
            _btRunner = this.GetComponent<BehaviorTreeRunner>();
            _exampleFSM = this.GetComponent<ExampleAIFSM>();
        }


        private void Start()
        {
            _unit = this.GetComponentInParent<Unit>();
            _gridMovement = _unit.GridMovement;
            _tacticsGrid = _unit.TacticsGrid;
            _abilitySystem = this.GetComponentInParent<IAbilitySystem>().AbilitySystem;

            CombatManager.Instance.OnCombatFinishing += CombatManager_OnCombatFinishing;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
        }

        private void CombatManager_OnCombatEnded()
        {
        }

        private void CombatManager_OnCombatFinishing(int winTeam)
        {
            StopAllCoroutines();
            _exampleFSM.OnEndTurn -= ExampleAIFSM_OnEndTurn;
            _btRunner.OnBehaviorFinished -= BehaviorTreeRunner_OnBehaviorFinished;

            _btRunner.BehaviorTree.Traverse(_btRunner.BehaviorTree.RootNode, (n) => n.Abort());
        }

        public void RunAILogic()
        {
            switch (_aiLogic)
            {
                case AILogic.FSM:
                    _exampleFSM.OnEndTurn += ExampleAIFSM_OnEndTurn;
                    StartCoroutine(_exampleFSM.SetAIState(AIState.StartingTurn));
                    break;
                case AILogic.BehaviorTree:
                    _btRunner.OnBehaviorFinished += BehaviorTreeRunner_OnBehaviorFinished;
                    StartCoroutine(_btRunner.RunBehavior());
                    break;
            }
        }

        private void ExampleAIFSM_OnEndTurn()
        {
                _exampleFSM.OnEndTurn -= ExampleAIFSM_OnEndTurn;
                StopAllCoroutines();
                _unit.AIEndTurn();
        }

        private void BehaviorTreeRunner_OnBehaviorFinished()
        {
            _btRunner.OnBehaviorFinished -= BehaviorTreeRunner_OnBehaviorFinished;
            StopAllCoroutines();
            _unit.AIEndTurn();
        }
    }
}