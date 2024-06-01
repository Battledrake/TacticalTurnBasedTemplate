using System;
using System.Collections;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class PlayAnimationTask : AbilityTask
    {
        public event Action<PlayAnimationTask, AbilityActivationData> OnAnimationEvent;
        public event Action<PlayAnimationTask, AbilityActivationData> OnAnimationCancelled;

        private IPlayAnimation _animatee;
        private AnimationType _animationType;
        private AnimationEventHandler _animEventHandler;
        private float _timeBeforeCancelling = 5f;
        private AbilityActivationData _activationData;

        public void InitTask(AbilityActivationData activationData, IPlayAnimation animatee, AnimationType animationType, float timeBeforeCancelling = 5f)
        {
            _activationData = activationData;

            if (animatee == null)
                return;

            _animatee = animatee;

            _timeBeforeCancelling = timeBeforeCancelling;

            _animationType = animationType;
            _animEventHandler = animatee.GetAnimationEventHandler();
        }

        private void OnAnimationEvent_OnAnimationCompleted()
        {
            AbilityTaskCompleted();
        }

        private void OnAnimationEvent_OnAnimationEvent()
        {
            OnAnimationEvent?.Invoke(this, _activationData);
        }

        public override IEnumerator ExecuteTask()
        {
            if (_animatee == null || !_animEventHandler)
            {
                Debug.LogWarning("Instigator does not have an IPlayAnimation and/or AnimationEventHandler. Cancelling Animation Task");
                OnAnimationCancelled?.Invoke(this, _activationData);
                EndTask();
                yield break;
            }

            _animatee.PlayAnimationType(_animationType);

            _animEventHandler.OnAnimationEvent += OnAnimationEvent_OnAnimationEvent;
            _animEventHandler.OnAnimationCompleted += OnAnimationEvent_OnAnimationCompleted;

            yield return new WaitForSeconds(_timeBeforeCancelling);
            if (this == null)
                yield break;
            Debug.LogWarning("Instigator did not receive AnimationEvent. Cancelling Animation Task");
            OnAnimationCancelled?.Invoke(this, _activationData);
            EndTask();
        }

        private void OnDestroy()
        {
            if(_animEventHandler != null)
            {
                _animEventHandler.OnAnimationEvent -= OnAnimationEvent_OnAnimationEvent;
                _animEventHandler.OnAnimationCompleted -= OnAnimationEvent_OnAnimationCompleted;
            }
        }
    }
}