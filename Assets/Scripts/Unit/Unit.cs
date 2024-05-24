using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    [RequireComponent(typeof(GridMovement))]
    public class Unit : MonoBehaviour, IUnitAnimation
    {
        public static event Action<Unit, GridIndex> OnUnitReachedNewTile;
        public event Action<Unit> OnUnitReachedDestination;
        public event Action<Unit> OnUnitStartedMovement;
        public event Action<Unit> OnUnitDied;
        public event Action<Unit> OnUnitRespawn;

        [SerializeField] private UnitId _unitType = UnitId.Ranger;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _selectedColor = Color.green;

        public GridIndex UnitGridIndex { get => _gridIndex; set => _gridIndex = value; }
        public UnitData UnitData { get => _unitData; }
        public bool IsMoving { get => _gridMovement.IsMoving; }

        private GameObject _unitVisual;
        private Animator _unitAnimator;
        private UnitData _unitData;
        private TacticsGrid _tacticsGrid;

        //TODO: Should we have a component for attributes?
        private float _currentHealth;
        private float _maxHealth;
        private float _moveRange;

        private GridIndex _gridIndex = GridIndex.Invalid();

        private GridMovement _gridMovement;

        //Outline Stuff
        private Outline _unitOutline;
        private float _defaultOutlineWidth = 2f;
        private float _hoverSelectedWidth = 3f;
        private bool _isHovered = false;
        private bool _isSelected = false;

        private void Awake()
        {
            _gridMovement = this.GetComponent<GridMovement>();
        }
        private void OnEnable()
        {
            _gridMovement.OnMovementStarted += GridMovement_OnMovementStarted;
            _gridMovement.OnReachedNewTile += GridMovement_OnReachedNewTile;
            _gridMovement.OnReachedDestination += GridMovement_OnReachedDestination;
        }
        private void OnDisable()
        {
            _gridMovement.OnMovementStarted -= GridMovement_OnMovementStarted;
            _gridMovement.OnReachedNewTile -= GridMovement_OnReachedNewTile;
            _gridMovement.OnReachedDestination -= GridMovement_OnReachedDestination;
        }

        private void GridMovement_OnReachedDestination()
        {
            OnUnitReachedDestination?.Invoke(this);
        }

        private void GridMovement_OnReachedNewTile(GridIndex index)
        {
            OnUnitReachedNewTile?.Invoke(this, index);
        }

        private void GridMovement_OnMovementStarted()
        {
            OnUnitStartedMovement?.Invoke(this);
        }

        public void SetUnitsGrid(TacticsGrid grid)
        {
            _tacticsGrid = grid;
            _gridMovement.SetPathingGrid(grid);
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
            _currentHealth = _unitData.unitStats.currentHealth;
            _maxHealth = _unitData.unitStats.maxHealth;
            _moveRange = _unitData.unitStats.moveRange;

            _unitAnimator = _unitVisual.GetComponent<Animator>();
            _unitOutline = _unitVisual.GetComponent<Outline>();
        }

        [ContextMenu("ChangeType")]
        public void ChangeUnitType()
        {
            InitializeUnit(_unitType);
        }

        [ContextMenu("ReAddToCombat")]
        public void AddUnitToCombat()
        {
            CombatSystem.Instance.AddUnitToCombat(this.transform.position, this);
            _unitAnimator.SetTrigger("Respawn");
            _currentHealth = _maxHealth;
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

        public void ApplyEffect(AbilityEffectReal effect)
        {
            switch (effect.attributeType)
            {
                case AttributeType.CurrentHealth:
                    ModifyCurrentHealth(effect.modifier);
                    break;
                case AttributeType.MaxHealth:
                    break;
                case AttributeType.MoveRange:
                    break;
            }
        }

        public void ApplyEffects(List<AbilityEffectReal> effects)
        {
            for(int i = 0; i < effects.Count; i++)
            {
                ApplyEffect(effects[i]);
            }
        }

        private void ModifyCurrentHealth(int effectModifier)
        {
            if (_currentHealth == 0 && effectModifier < 0)
                return;
            if (_currentHealth == _maxHealth && effectModifier > 0)
                return;

            _currentHealth = Mathf.Clamp(_currentHealth + effectModifier, 0, _maxHealth);

            if (_currentHealth == 0)
            {
                PlayDeathAnimation();
                OnUnitDied?.Invoke(this);
                return;
            }

            if (effectModifier < 0)
                TriggerHitAnimation();
        }

        public void UseAbility(GridIndex targetIndex)
        {
            _tacticsGrid.GetTileDataFromIndex(targetIndex, out TileData tileData);
            Vector3 lookAtVector = tileData.tileMatrix.GetPosition();
            lookAtVector.y = this.transform.position.y;
            this.transform.LookAt(lookAtVector);

            _unitAnimator.ResetTrigger("Attack");
            _unitAnimator.SetTrigger("Attack");
        }

        public void TriggerHitAnimation()
        {
            _unitAnimator.SetTrigger("Hit");
        }

        public void PlayDeathAnimation()
        {
            _unitAnimator.SetTrigger("Die");
        }

        //public void SetPathAndMove(List<GridIndex> path)
        //{
        //    _currentPathToFollow = new List<GridIndex>(path);
        //    _isMoving = true;
        //    _unitAnimator.SetFloat("Speed", _moveSpeed);
        //    _unitAnimator.speed = _moveSpeed;
        //    UpdatePath();

        //    OnUnitStartedMovement?.Invoke(this);
        //}
    }
}
