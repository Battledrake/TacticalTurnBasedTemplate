using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public abstract class AbilityTask : MonoBehaviour
    {
        public event Action<AbilityTask> OnTaskCompleted;

        protected bool _isRunning = false;

        public abstract IEnumerator ExecuteTask(Ability owner);
        public virtual void EndTask()
        {
            _isRunning = false;
            Destroy(this.gameObject);
        }

        protected void AbilityTaskCompleted()
        {
            OnTaskCompleted?.Invoke(this);
            EndTask();
        }
    }
}