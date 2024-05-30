using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AnimateObjectTask : AbilityTask
    {
        public event Action<AnimateObjectTask> OnInitialAnimationCompleted;
        public event Action<Unit> OnObjectCollisionWithUnit;

        private GameObject _objectToAnimate;

        private float _animationTime = 1f;
        private float _animationSpeed = 1f;
        private bool _loopAnimation = false;

        private AnimationCurve _positionAlphaCurve;
        private AnimationCurve _pivotYawCurve;
        private AnimationCurve _pivotPitchCurve;
        private AnimationCurve _pivotRollCurve;
        private AnimationCurve _objectPosXCurve;
        private AnimationCurve _objectPosYCurve;
        private AnimationCurve _objectYawCurve;
        private AnimationCurve _objectPitchCurve;
        private AnimationCurve _objectRollCurve;
        private AnimationCurve _uniformScaleCurve;
        private AnimationCurve _scaleXCurve;
        private AnimationCurve _scaleYCurve;
        private AnimationCurve _scaleZCurve;
        private AnimationCurve _opacityCurve;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private Vector3 _direction;

        private float _timeElapsed = 0f;
        private bool _hasInitiallyLooped = false;

        public void InitTask(GameObject objectToAnimate, AnimateObjectTaskData taskData, Vector3 startPosition, Vector3 targetPosition, float animationTime = 1f, float animationSpeed = 1f, bool loopAnimation = false)
        {
            _objectToAnimate = objectToAnimate;
            _objectToAnimate.transform.parent = this.transform;

            _animationTime = animationTime;
            //_animationTime = taskData.animationTime;
            _animationSpeed = animationSpeed;

            _positionAlphaCurve = taskData.positionAlphaCurve;
            _pivotYawCurve = taskData.pivotYawCurve;
            _pivotPitchCurve = taskData.pivotPitchCurve;
            _pivotRollCurve = taskData.pivotRollCurve;
            _objectPosXCurve = taskData.objectPosXCurve;
            _objectPosYCurve = taskData.objectPosYCurve;
            _objectYawCurve = taskData.objectYawCurve;
            _objectPitchCurve = taskData.objectPitchCurve;
            _objectRollCurve = taskData.objectRollCurve;
            _uniformScaleCurve = taskData.uniformScaleCurve;
            _scaleXCurve = taskData.scaleXCurve;
            _scaleYCurve = taskData.scaleYCurve;
            _scaleZCurve = taskData.scaleZCurve;
            _opacityCurve = taskData.opacityCurve;

            _loopAnimation = loopAnimation;

            _startPosition = startPosition;
            _targetPosition = targetPosition;
            _direction = _targetPosition - _startPosition;
            _direction.Normalize();

            this.transform.position = _startPosition;
            this.transform.LookAt(_targetPosition);

            _isRunning = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit)
            {
                OnObjectCollisionWithUnit?.Invoke(unit);
            }
        }

        public override IEnumerator ExecuteTask(Ability owner)
        {
            while (_isRunning)
            {
                _timeElapsed += Time.deltaTime;

                if (_timeElapsed >= _animationTime)
                {
                    if (!_hasInitiallyLooped)
                    {
                        _hasInitiallyLooped = true;
                        OnInitialAnimationCompleted?.Invoke(this);
                    }
                    if (_loopAnimation)
                    {
                        _timeElapsed = 0f;
                    }
                    else
                    {
                        AbilityTaskCompleted();
                    }
                }

                float evalSpeed = _timeElapsed * _animationSpeed;

                if (_positionAlphaCurve.length > 0)
                {
                    this.transform.position = Vector3.LerpUnclamped(_startPosition, _targetPosition, _positionAlphaCurve.Evaluate(evalSpeed));
                }


                float pivotX = this.transform.rotation.eulerAngles.x;
                float pivotY = this.transform.rotation.eulerAngles.y;
                float pivotZ = this.transform.rotation.eulerAngles.z;

                if (_pivotPitchCurve.length > 0)
                {
                    pivotX = _pivotPitchCurve.Evaluate(evalSpeed);
                }

                if (_pivotYawCurve.length > 0)
                {
                    pivotY = _pivotYawCurve.Evaluate(evalSpeed);
                }

                if (_pivotRollCurve.length > 0)
                {
                    pivotZ = _pivotRollCurve.Evaluate(evalSpeed);
                }

                this.transform.rotation = Quaternion.Euler(pivotX, pivotY, pivotZ);

                float objectXPosition = _objectToAnimate.transform.localPosition.x;
                float objectYPosition = _objectToAnimate.transform.localPosition.y;
                float objectZPosition = _objectToAnimate.transform.localPosition.z;

                if(_objectPosXCurve.length > 0)
                {
                    objectXPosition = _objectPosXCurve.Evaluate(evalSpeed);
                }
                if(_objectPosYCurve.length > 0)
                {
                    objectYPosition = _objectPosYCurve.Evaluate(evalSpeed);
                }
                _objectToAnimate.transform.localPosition = new Vector3(objectXPosition, objectYPosition, objectZPosition);

                float objectXRotation = _objectToAnimate.transform.localRotation.eulerAngles.x;
                float objectYRotation = _objectToAnimate.transform.localRotation.eulerAngles.y;
                float objectZRotation = _objectToAnimate.transform.localRotation.eulerAngles.z;

                if (_objectPitchCurve.length > 0)
                {
                    objectXRotation = _objectPitchCurve.Evaluate(evalSpeed);
                }

                if (_objectYawCurve.length > 0)
                {
                    objectYRotation = _objectYawCurve.Evaluate(evalSpeed);
                }

                if (_objectRollCurve.length > 0)
                {
                    objectZRotation = _objectRollCurve.Evaluate(evalSpeed);
                }

                _objectToAnimate.transform.localRotation = Quaternion.Euler(objectXRotation, objectYRotation, objectZRotation);

                if (_uniformScaleCurve.length > 0)
                {
                    _objectToAnimate.transform.localScale = _uniformScaleCurve.Evaluate(evalSpeed) * Vector3.one;
                }

                Vector3 objectScale = _objectToAnimate.transform.localScale;
                if (_scaleXCurve.length > 0)
                {
                    objectScale.x = _scaleXCurve.Evaluate(evalSpeed);
                }
                if (_scaleYCurve.length > 0)
                {
                    objectScale.y = _scaleYCurve.Evaluate(evalSpeed);
                }
                if (_scaleZCurve.length > 0)
                {
                    objectScale.z = _scaleZCurve.Evaluate(evalSpeed);
                }

                _objectToAnimate.transform.localScale = new Vector3(objectScale.x, objectScale.y, objectScale.z);

                if (_opacityCurve.length > 0)
                {
                    MeshRenderer[] renderComponents = _objectToAnimate.GetComponentsInChildren<MeshRenderer>();
                    for (int i = 0; i < renderComponents.Length; i++)
                    {
                        Color materialColor = renderComponents[i].material.color;
                        materialColor.a = _opacityCurve.Evaluate(evalSpeed);
                        renderComponents[i].material.color = materialColor;
                    }
                    SpriteRenderer[] spriteRenderers = _objectToAnimate.GetComponentsInChildren<SpriteRenderer>();
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        Color imageColor = spriteRenderers[i].color;
                        imageColor.a = _opacityCurve.Evaluate(evalSpeed);
                        spriteRenderers[i].color = imageColor;
                    }
                }
                yield return null;
            }
        }
    }
}