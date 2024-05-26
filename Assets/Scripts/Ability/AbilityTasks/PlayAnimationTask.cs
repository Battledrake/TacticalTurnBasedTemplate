using System;
using System.Collections;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class PlayAnimationTask : AbilityTask
    {
        public event Action<PlayAnimationTask> OnAnimationEvent;
        public event Action<PlayAnimationTask> OnAnimationCancelled;

        private Animator _unitAnimator;
        private AnimationType _animationType;
        private AnimationEventHandler _animEventListener;

        private float _timeBeforeCancelling = 5f;

        public void InitTask(Unit unit, AnimationType animationType, float timeBeforeCancelling = 5f)
        {
            if (!unit)
                return;
            _timeBeforeCancelling = timeBeforeCancelling;

            _unitAnimator = unit.GetComponentInChildren<Animator>();
            _animationType = animationType;
            _animEventListener = unit.GetComponentInChildren<AnimationEventHandler>();
        }

        private void OnAnimationEvent_OnAnimationEnd()
        {
            AbilityTaskCompleted();
        }

        private void OnAnimationEvent_OnAnimationEvent()
        {
            OnAnimationEvent?.Invoke(this);
        }

        public override IEnumerator ExecuteTask(Ability owner)
        {
            owner.OnAbilityEnd += EndTask;

            if (!_unitAnimator && !_animEventListener)
            {
                OnAnimationCancelled?.Invoke(this);
                yield break;
            }

            _unitAnimator.SetTrigger(_animationType.ToString());

            _animEventListener.OnAnimationEvent += OnAnimationEvent_OnAnimationEvent;
            _animEventListener.OnAnimationCompleted += OnAnimationEvent_OnAnimationEnd;

            yield return new WaitForSeconds(_timeBeforeCancelling);
            OnAnimationCancelled?.Invoke(this);
        }

        private void OnDestroy()
        {
            _animEventListener.OnAnimationEvent -= OnAnimationEvent_OnAnimationEvent;
            _animEventListener.OnAnimationCompleted -= OnAnimationEvent_OnAnimationEnd;
        }
    }
}