using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class Unit : MonoBehaviour, IUnitAnimation
    {
        public static event Action<Unit, GridIndex> OnUnitReachedNewTile;
        public event Action<Unit> OnUnitReachedDestination;
        public event Action<Unit> OnUnitStartedMovement;

        [SerializeField] private UnitId _unitType = UnitId.Ranger;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _selectedColor = Color.green;
        [SerializeField] private AnimationCurve _positionAlpha;
        [SerializeField] private AnimationCurve _rotationAlpha;
        [SerializeField] private AnimationCurve _jumpCurve;

        public GridIndex UnitGridIndex { get => _gridIndex; set => _gridIndex = value; }
        public UnitData UnitData { get => _unitData; }

        private GameObject _unitVisual;
        private Animator _unitAnimator;
        private UnitData _unitData;
        private TacticsGrid _tacticsGrid;

        //TODO: Movement stuff inside its own component like navagent
        private GridIndex _gridIndex = GridIndex.Invalid();
        private List<GridIndex> _currentPathToFollow;
        private bool _isMoving;
        [SerializeField] private float _moveSpeed = 3f;
        private Matrix4x4 _previousTransform;
        private Matrix4x4 _nextTransform;
        private float _moveTimer = 0f;

        //Outline Stuff
        private Outline _unitOutline;
        private float _defaultOutlineWidth = 2f;
        private float _hoverSelectedWidth = 3f;
        private bool _isHovered = false;
        private bool _isSelected = false;

        public void SetUnitsGrid(TacticsGrid grid)
        {
            _tacticsGrid = grid;
        }

        public void InitializeUnit(UnitId unitType)
        {
            if (_unitVisual != null)
                Destroy(_unitVisual);

            _unitType = unitType;
            _unitData = DataManager.GetUnitDataFromType(_unitType);
            if (_unitVisual != null)
                Destroy(_unitVisual);
            _unitVisual = Instantiate(_unitData.assetData.unitVisual, this.transform);
            _moveSpeed = _unitData.unitStats.moveSpeed;

            _unitAnimator = _unitVisual.GetComponent<Animator>();
            _unitOutline = _unitVisual.GetComponent<Outline>();
        }

        private void Update()
        {
            if (_isMoving)
            {
                if (_moveTimer >= 1f)
                {
                    OnUnitReachedNewTile?.Invoke(this, _currentPathToFollow[0]);
                    _currentPathToFollow.RemoveAt(0);
                    UpdatePath();
                }
                else
                {
                    _moveTimer += _moveSpeed * Time.deltaTime;
                }
                Vector3 jumpVector = Vector3.zero;
                if (ShouldJumpToNextTile())
                    jumpVector.y = _jumpCurve.Evaluate(_moveTimer);
                this.transform.position = Vector3.Lerp(_previousTransform.GetPosition(), _nextTransform.GetPosition() + jumpVector, _positionAlpha.Evaluate(_moveTimer));
                this.transform.rotation = Quaternion.Slerp(_previousTransform.rotation, _nextTransform.rotation, _rotationAlpha.Evaluate(_moveTimer));
            }
        }

        private void UpdatePath()
        {
            if(_currentPathToFollow.Count > 0)
            {
                _previousTransform = this.transform.localToWorldMatrix;
                Matrix4x4 nextTransform = _tacticsGrid.GridTiles[_currentPathToFollow[0]].tileMatrix;
                Vector3 lookVector = nextTransform.GetPosition() - this.transform.position;
                lookVector.y = 0f;
                Quaternion lookRotation = Quaternion.LookRotation(lookVector, Vector3.up);

                _nextTransform = Matrix4x4.TRS(nextTransform.GetPosition(), lookRotation, nextTransform.lossyScale);
                _moveTimer = 0f;
            }
            else
            {
                _isMoving = false;
                _unitAnimator.SetFloat("Speed", 0f);
                _unitAnimator.speed = 1f;
                OnUnitReachedDestination?.Invoke(this);
            }
        }

        private bool ShouldJumpToNextTile()
        {
            float previousY = this.transform.localToWorldMatrix.GetPosition().y;
            float nextY = _nextTransform.GetPosition().y;

            return Mathf.Abs(nextY - previousY) > 0.2f;
        }

        [ContextMenu("ChangeType")]
        public void ChangeUnitType()
        {
            InitializeUnit(_unitType);
        }

        [ContextMenu("AddToCombat")]
        public void AddUnitToCombat()
        {
            GameObject.Find("[CombatSystem]").GetComponent<CombatSystem>().AddUnitToCombat(this.transform.position, this);
            _unitAnimator.SetTrigger("Respawn");
        }

        public void SetIsHovered(bool isHovered)
        {
            _isHovered = isHovered;
            UpdateOutlineVisual();
        }

        public void SetIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            UpdateOutlineVisual();
        }

        public void UpdateOutlineVisual()
        {
            if (!_unitOutline)
                return;

            if (!_isSelected && !_isHovered)
            {
                _unitOutline.enabled = false;
                return;
            }
            _unitOutline.enabled = true;

            if (_isSelected)
            {
                _unitOutline.OutlineColor = _selectedColor;

                if (_isHovered)
                    _unitOutline.OutlineWidth = _hoverSelectedWidth;
                else
                    _unitOutline.OutlineWidth = _defaultOutlineWidth;
            }
            else
            {
                _unitOutline.OutlineColor = _hoverColor;
                _unitOutline.OutlineWidth = _defaultOutlineWidth;
            }
        }

        public void PlayDeathAnimation()
        {
            _unitAnimator.SetTrigger("Die");
        }

        public void SetPathAndMove(List<GridIndex> path)
        {
            _currentPathToFollow = new List<GridIndex>(path);
            _isMoving = true;
            _unitAnimator.SetFloat("Speed", _moveSpeed);
            _unitAnimator.speed = _moveSpeed;
            UpdatePath();

            OnUnitStartedMovement?.Invoke(this);
        }
    }
}
