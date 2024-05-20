using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlashAbility : Ability
{
    [SerializeField] private GameObject _swordPrefab;
    [SerializeField] private AnimationCurve _rotationCurve;
    [SerializeField] private AnimationCurve _scaleCurve;
    [SerializeField] private float _animationSpeed;

    private float _timeElapsed = 0f;
    protected void ExecuteTask()
    {
        Debug.Log(_targetIndex);
    }

    public void Update()
    {
        _timeElapsed += Time.deltaTime * _animationSpeed;
        if(_timeElapsed >= _rotationCurve[_rotationCurve.length - 1].time)
        {
            AbilityBehaviorComplete(this);
            EndAbility();
        }
        this.transform.rotation = Quaternion.Euler(_rotationCurve.Evaluate(_timeElapsed), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        this.transform.localScale = _scaleCurve.Evaluate(_timeElapsed) * Vector3.one;
    }

    public override bool CanActivateAbility()
    {
        return true;
    }

    protected override void CommitAbility()
    {
    }

    public override void ActivateAbility()
    {
        _tacticsGrid.GetTileDataFromIndex(_originIndex, out TileData originData);
        _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);

        this.transform.position = originData.tileMatrix.GetPosition();
        this.transform.LookAt(targetData.tileMatrix.GetPosition());
    }

    public override bool TryActivateAbility()
    {
        if (CanActivateAbility())
            ActivateAbility();

        return (CanActivateAbility());
    }

    public override void EndAbility()
    {
        Destroy(this.gameObject);
    }
}
