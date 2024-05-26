using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public interface IUnitAnimation
{
    public void PlayAnimationType(AnimationType animationType) { }
    public void PlayAttackAnimation() { }
    public void PlayDeathAnimation() { }
    public void TriggerHitAnimation() { }
}
