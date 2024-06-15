using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        public event Action OnBehaviorFinished;

        [SerializeField] private BehaviorTree _behaviorTree;
        [SerializeField] private float _delayBetweenEvaluations = 0.0f;

        public BehaviorTree BehaviorTree => _behaviorTree;

        private void Start()
        {
            if (_behaviorTree != null)
            {
                _behaviorTree = _behaviorTree.Clone();

                _behaviorTree.Bind(GetComponent<IBehaviorTreeAgent>());
            }
        }

        //private void Start()
        //{
        //    StartCoroutine(RunBehavior());
        //}

        public IEnumerator RunBehavior()
        {
            while (_behaviorTree.ExecuteTree() == NodeResult.Running)
            {
                yield return new WaitForSeconds(_delayBetweenEvaluations);
            }
            OnBehaviorFinished?.Invoke();
        }
    }
}
