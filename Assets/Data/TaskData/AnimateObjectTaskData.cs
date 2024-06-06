using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimateObjectTaskData", menuName = "TTBT/Ability/TaskData/AnimateObject")]
public class AnimateObjectTaskData : ScriptableObject
{
    //public float animationTime = 1f;
    //public float animationSpeed = 1f;

    [Header("Main Position Alpha")]
    public AnimationCurve positionAlphaCurve;
    [Header("Pivot Yaw")]
    public AnimationCurve pivotYawCurve;
    [Header("Pivot Pitch")]
    public AnimationCurve pivotPitchCurve;
    [Header("Pivot Roll")]
    public AnimationCurve pivotRollCurve;
    [Header("Object Position X-Axis")]
    public AnimationCurve objectPosXCurve;
    [Header("Object Position Y-Axis")]
    public AnimationCurve objectPosYCurve;
    [Header("Object Yaw")]
    public AnimationCurve objectYawCurve;
    [Header("Object Pitch")]
    public AnimationCurve objectPitchCurve;
    [Header("Object Roll")]
    public AnimationCurve objectRollCurve;
    [Header("Uniform Scale")]
    public AnimationCurve uniformScaleCurve;
    [Header("Scale X Only")]
    public AnimationCurve scaleXCurve;
    [Header("Scale Y Only")]
    public AnimationCurve scaleYCurve;
    [Header("Scale Z Only")]
    public AnimationCurve scaleZCurve;
    [Header("Opacity")]
    public AnimationCurve opacityCurve;

    [ContextMenu("ClearKeys/Clear Position Alpha Keys", false, 0)]
    public void ClearPositionAlphaCurve()
    {
        positionAlphaCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Pivot Yaw Keys", false, 1)]
    public void ClearPivotYawCurve()
    {
        pivotYawCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Pivot Pitch Keys", false, 2)]
    public void ClearPivotPitchKeys()
    {
        pivotPitchCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Pivot Roll Keys", false, 3)]
    public void ClearPivotRollCurve()
    {
        pivotRollCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Object Position X Keys", false, 4)]
    public void ClearObjectPosXKeys()
    {
        objectPosXCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Object Position Y Keys", false, 5)]
    public void ClearObjectPosYKeys()
    {
        objectPosYCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Object Yaw Keys", false, 6)]
    public void ClearObjectYawCurve()
    {
        objectYawCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Object Pitch Keys", false, 7)]
    public void ClearObjectPitchCurve()
    {
        objectPitchCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Object Roll Keys", false, 8)]
    public void ClearObjectRollCurve()
    {
        objectRollCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Uniform Scale Keys", false, 9)]
    public void ClearScaleCurve()
    {
        uniformScaleCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Scale X Keys", false, 10)]
    public void ClearScaleXCurve()
    {
        scaleXCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Scale Y Keys", false, 11)]
    public void ClearScaleYCurve()
    {
        scaleYCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Scale Z Keys", false, 12)]
    public void ClearScaleZCurve()
    {
        scaleZCurve.ClearKeys();
    }
    [ContextMenu("ClearKeys/Clear Opacity Keys", false, 13)]
    public void ClearOpacityCurve()
    {
        opacityCurve.ClearKeys();
    }
}
