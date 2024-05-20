using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class ShortRangeArrowAbility : Ability
    {
        [SerializeField] private float _animationTime = 1f;
        [SerializeField] private float _animationSpeed = 1f;

        [SerializeField] private AnimationCurve _positionCurve;
        [SerializeField] private AnimationCurve _positionYCurve;
        [SerializeField] private AnimationCurve _rotationXCurve;
        [SerializeField] private AnimationCurve _scaleCurve;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private float _timeElapsed = 0f;
        private bool _isActive = false;

        void Update()
        {
            if (!_isActive)
                return;
            _timeElapsed += Time.deltaTime * _animationSpeed;

            if (_timeElapsed > _animationTime)
            {
                _isActive = false;
                EndAbility();
            }

            if (_positionCurve.length > 0)
                this.transform.position = Vector3.LerpUnclamped(_startPosition, _targetPosition, _positionCurve.Evaluate(_timeElapsed));


            if (_positionYCurve.length > 0)
            {
                float YOffset = _positionYCurve.Evaluate(_timeElapsed);
                Vector3 newPosition = this.transform.position + Vector3.up * YOffset;
                this.transform.position = newPosition;
            }

            if (_rotationXCurve.length > 0)
                this.transform.rotation = Quaternion.Euler(_rotationXCurve.Evaluate(_timeElapsed), this.transform.eulerAngles.y, this.transform.eulerAngles.z);

            if (_scaleCurve.length > 0)
                this.transform.localScale = _scaleCurve.Evaluate(_timeElapsed) * Vector3.one;

        }
        public override void ActivateAbility()
        {
            _tacticsGrid.GetTileDataFromIndex(_originIndex, out TileData originData);
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);

            _startPosition = originData.tileMatrix.GetPosition();
            _targetPosition = targetData.tileMatrix.GetPosition();

            this.transform.position = originData.tileMatrix.GetPosition();
            this.transform.LookAt(targetData.tileMatrix.GetPosition());

            _isActive = true;
        }

        public override bool CanActivateAbility()
        {
            return true;
        }

        public override void EndAbility()
        {
            AbilityBehaviorComplete(this);
            Destroy(this.gameObject);
        }

        public override bool TryActivateAbility()
        {
            if (CanActivateAbility())
            {
                ActivateAbility();
                return true;
            }
            return false;
        }

        protected override void CommitAbility()
        {
        }
    }
}