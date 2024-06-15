using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum AnimationType
    {
        Idle,
        Run,
        Attack,
        Cast,
        Hit,
        Death,
        Respawn
    }

    public interface IPlayAnimation
    {
        public AnimationEventHandler AnimationEventHandler { get; }

        public void PlayAnimationType(AnimationType animationType) { }
    }
}
