using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitAnimationState
{
    Idle,
    Walk,
    Attack,
    Hit,
    Death,
    Respawn
}

public interface IUnitAnimation
{
    public void PlayDeathAnimation() { }
}
