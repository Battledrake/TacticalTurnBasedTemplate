using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class AbilityTask : MonoBehaviour
    {
        public event Action OnTaskCompleted;

        protected bool _isRunning = false;

        public abstract IEnumerator ExecuteTask();
        public virtual void EndTask()
        {
            _isRunning = false;
            StopCoroutine(ExecuteTask());
            AbilityTaskCompleted();
            Destroy(this.gameObject);
        }

        protected void AbilityTaskCompleted()
        {
            OnTaskCompleted?.Invoke();
        }
    }
}