using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AnimationEventHandler : MonoBehaviour
    {
        public event Action OnAnimationEvent;
        public event Action OnAnimationCompleted;

        public void AnimationEvent()
        {
            OnAnimationEvent?.Invoke();
        }

        public void AnimationCompleted()
        {
            OnAnimationCompleted?.Invoke();
        }
    }
}