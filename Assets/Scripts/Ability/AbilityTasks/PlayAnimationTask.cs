using System;
using System.Collections;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class PlayAnimationTask : AbilityTask
    {
        public event Action<PlayAnimationTask> OnAnimationEvent;

        private bool _animationCompleted = false;

        private Animator _unitAnimator;
        private string _animationTrigger;
        private AnimEventListener _animEventListener;

        public void InitTask(Unit unit, string animationTrigger)
        {
            _unitAnimator = unit.GetComponentInChildren<Animator>();
            _animationTrigger = animationTrigger;
            _animEventListener = unit.GetComponentInChildren<AnimEventListener>();
        }

        private void OnAnimationEvent_OnAnimationEnd()
        {
            _animationCompleted = true;
            AbilityTaskCompleted();
        }

        private void OnAnimationEvent_OnAnimationEvent()
        {
            OnAnimationEvent?.Invoke(this);
        }

        public override IEnumerator ExecuteTask(Ability owner)
        {
            owner.OnAbilityEnd += EndTask;

            if (!_unitAnimator && !_animEventListener && String.IsNullOrEmpty(_animationTrigger))
                yield break;

            _unitAnimator.SetTrigger(_animationTrigger);

            _animEventListener.OnAnimationEvent += OnAnimationEvent_OnAnimationEvent;
            _animEventListener.OnAnimationCompleted += OnAnimationEvent_OnAnimationEnd;

            yield return new WaitUntil(() => _animationCompleted);
            AbilityTaskCompleted();
        }

        private void OnDestroy()
        {
            _animEventListener.OnAnimationEvent -= OnAnimationEvent_OnAnimationEvent;
            _animEventListener.OnAnimationCompleted -= OnAnimationEvent_OnAnimationEnd;
        }
    }
}