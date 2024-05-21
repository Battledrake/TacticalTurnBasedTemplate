using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AnimatedObjectAbility : Ability
    {
        [SerializeField] private GameObject _objectToAnimate;

        [SerializeField] private float _animationTime = 1f;
        [SerializeField] private float _animationSpeed = 1f;
        [SerializeField] private bool _loopAnimation = false;

        [Header("Core Position Alpha")]
        [SerializeField] private AnimationCurve _positionCurve;
        [Header("Core Position Direction X-Axis")]
        [SerializeField] private AnimationCurve _positionXCurve;
        [Header("Core Position Direction Y-Axis")]
        [SerializeField] private AnimationCurve _positionYCurve;
        [Header("Core Rotation Yaw")]
        [SerializeField] private AnimationCurve _coreYawCurve;
        [Header("Core Rotation Pitch")]
        [SerializeField] private AnimationCurve _corePitchCurve;
        [Header("Core Rotation Roll")]
        [SerializeField] private AnimationCurve _coreRollCurve;
        [Header("Object Rotation Yaw")]
        [SerializeField] private AnimationCurve _objectYawCurve;
        [Header("Object Rotation Pitch")]
        [SerializeField] private AnimationCurve _objectPitchCurve;
        [Header("Object Rotation Roll")]
        [SerializeField] private AnimationCurve _objectRollCurve;
        [Header("Scale")]
        [SerializeField] private AnimationCurve _scaleCurve;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Vector3 _direction;

        private float _timeElapsed = 0f;
        private bool _isActive = false;
        private bool _hasInitiallyLooped = false;

        /// <summary>
        /// Process all the animation logic.
        /// </summary>
        void Update()
        {
            if (!_isActive)
                return;

            _timeElapsed += Time.deltaTime * _animationSpeed;

            if (_timeElapsed >= _animationTime)
            {
                if (!_hasInitiallyLooped)
                {
                    _hasInitiallyLooped = true;
                    AbilityBehaviorComplete(this);
                }
                if (_loopAnimation)
                {
                    _timeElapsed = 0f;
                }
                else
                {
                    _isActive = false;
                    EndAbility();
                }
            }

            if (_positionCurve.length > 0)
                this.transform.position = Vector3.LerpUnclamped(_startPosition, _targetPosition, _positionCurve.Evaluate(_timeElapsed));


            if(_positionXCurve.length > 0)
            {
                float xOffset = _positionXCurve.Evaluate(_timeElapsed);
                Vector3 rightVector = Vector3.Cross(Vector3.up, _direction);
                Vector3 xOffSetPosition = this.transform.position + rightVector * xOffset;
                this.transform.position = xOffSetPosition;
            }

            if (_positionYCurve.length > 0)
            {
                float YOffset = _positionYCurve.Evaluate(_timeElapsed);
                Vector3 upPosition = this.transform.position + Vector3.up * YOffset;
                this.transform.position = upPosition;
            }

            float coreX = this.transform.rotation.eulerAngles.x;
            float coreY = this.transform.rotation.eulerAngles.y;
            float coreZ = this.transform.rotation.eulerAngles.z;

            if (_corePitchCurve.length > 0)
            {
                coreX = _corePitchCurve.Evaluate(_timeElapsed);
            }

            if (_coreYawCurve.length > 0)
            {
                coreY = _coreYawCurve.Evaluate(_timeElapsed);
            }

            if (_coreRollCurve.length > 0)
            {
                coreZ = _coreRollCurve.Evaluate(_timeElapsed);
            }

            this.transform.rotation = Quaternion.Euler(coreX, coreY, coreZ);

            float objectXRotation = _objectToAnimate.transform.rotation.eulerAngles.x;
            float objectYRotation = _objectToAnimate.transform.rotation.eulerAngles.y;
            float objectZRotation = _objectToAnimate.transform.rotation.eulerAngles.z;

            if (_objectPitchCurve.length > 0)
            {
                objectXRotation = _objectPitchCurve.Evaluate(_timeElapsed);
            }

            if (_objectYawCurve.length > 0)
            {
                objectYRotation = _objectYawCurve.Evaluate(_timeElapsed);
            }

            if (_objectRollCurve.length > 0)
            {
                objectZRotation = _objectRollCurve.Evaluate(_timeElapsed);
            }

            _objectToAnimate.transform.rotation = Quaternion.Euler(objectXRotation, objectYRotation, objectZRotation);

            if (_scaleCurve.length > 0)
            {
                this.transform.localScale = _scaleCurve.Evaluate(_timeElapsed) * Vector3.one;
            }

        }

        /// <summary>
        /// Set values for update calls.
        /// </summary>
        public override void ActivateAbility()
        {
            _tacticsGrid.GetTileDataFromIndex(_originIndex, out TileData originData);
            _tacticsGrid.GetTileDataFromIndex(_targetIndex, out TileData targetData);

            _startPosition = originData.tileMatrix.GetPosition();
            _targetPosition = targetData.tileMatrix.GetPosition();

            _direction = _targetPosition - _startPosition;
            _direction.Normalize();

            this.transform.position = originData.tileMatrix.GetPosition();
            this.transform.LookAt(targetData.tileMatrix.GetPosition());

            _isActive = true;
        }

        /// <summary>
        /// Process Cooldown checks. Ensure tiles are valid for this ability.
        /// </summary>
        /// <returns></returns>
        public override bool CanActivateAbility()
        {
            return true;
        }

        public override void EndAbility()
        {
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Checks if the ability can be activated before activating
        /// </summary>
        /// <returns></returns>
        public override bool TryActivateAbility()
        {
            if (CanActivateAbility())
            {
                ActivateAbility();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Use AP, Resources, Item, Etc...
        /// </summary>
        protected override void CommitAbility()
        {
        }

        [ContextMenu("ClearKeys/Clear Position Keys", false, 0)]
        public void ClearPositionCurve()
        {
            _positionCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Position X Keys", false, 1)]
        public void ClearPositionXCurve()
        {
            _positionXCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Position Y Keys", false, 2)]
        public void ClearPositionYCurve()
        {
            _positionYCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Core Yaw Keys", false, 3)]
        public void ClearCoreYawCurve()
        {
            _coreYawCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Core Pitch Keys", false, 4)]
        public void ClearCorePitchCurve()
        {
            _corePitchCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Core Roll Keys", false, 5)]
        public void ClearCoreRollCurve()
        {
            _coreRollCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Object Yaw Keys", false, 6)]
        public void ClearObjectYawCurve()
        {
            _objectYawCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Object Pitch Keys", false, 7)]
        public void ClearObjectPitchCurve()
        {
            _objectPitchCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Object Roll Keys", false, 8)]
        public void ClearObjectRollCurve()
        {
            _objectRollCurve.ClearKeys();
        }
        [ContextMenu("ClearKeys/Clear Scale Keys", false, 9)]
        public void ClearScaleCurve()
        {
            _scaleCurve.ClearKeys();
        }
    }
}