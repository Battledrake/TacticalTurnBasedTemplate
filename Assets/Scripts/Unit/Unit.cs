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
        private GridIndex _gridIndex = GridIndex.Invalid();

        //TODO: Should we have a component for attributes?
        private int _currentHealth;
        private int _maxHealth;
        private int _moveRange;

        private TacticsGrid _tacticsGrid;
        private Collider _collider;
        private GridMovement _gridMovement;
        private Health _healthComponent;

        //Outline Stuff
        private Outline _unitOutline;
        private float _defaultOutlineWidth = 2f;
        private float _hoverSelectedWidth = 3f;
        private bool _isHovered = false;
        private bool _isSelected = false;

        private void Awake()
        {
            _collider = this.GetComponent<Collider>();
            _gridMovement = this.GetComponent<GridMovement>();
            _healthComponent = this.GetComponent<Health>();

            _healthComponent.OnHealthReachedZero += Die;
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
            //_unitAnimator.SetFloat("Speed", 0);
            //_unitAnimator.speed = 1f;
            _unitAnimator.SetTrigger(AnimationType.Idle.ToString());
        }

        private void GridMovement_OnReachedNewTile(GridIndex index)
        {
            OnUnitReachedNewTile?.Invoke(this, index);
        }

        private void GridMovement_OnMovementStarted()
        {
            OnUnitStartedMovement?.Invoke(this);
            _unitAnimator.SetTrigger(AnimationType.Run.ToString());
        }

        private void Update()
        {
            if (_gridMovement.IsMoving)
            {
                //_unitAnimator.SetFloat("Speed", _gridMovement.CurrentMovementSpeed);
                //_unitAnimator.speed = 2f;
            }
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
            _unitOutline = _unitVisual.AddComponent<Outline>();
            _unitVisual.AddComponent<AnimationEventHandler>();
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

            _unitAnimator.ResetTrigger("Hit");
            _unitAnimator.SetTrigger("Respawn");
            _currentHealth = _maxHealth;
            _collider.enabled = true;
            _healthComponent.UpdateHealth(_maxHealth);
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

        //TODO: Move this logic to ability component
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
            for (int i = 0; i < effects.Count; i++)
            {
                ApplyEffect(effects[i]);
            }
        }

        private void ModifyCurrentHealth(int effectModifier)
        {
            _healthComponent.UpdateHealth(effectModifier);

            //if (_currentHealth == 0 && effectModifier < 0)
            //    return;
            //if (_currentHealth == _maxHealth && effectModifier > 0)
            //    return;

            //_currentHealth = Mathf.Clamp(_currentHealth + effectModifier, 0, _maxHealth);

            //if (_currentHealth == 0)
            //{
            //    _collider.enabled = false;
            //    PlayDeathAnimation();
            //    OnUnitDied?.Invoke(this);
            //    return;
            //}

            if (effectModifier < 0)
                TriggerHitAnimation();
        }

        public void Die()
        {
            _collider.enabled = false;
            PlayDeathAnimation();
            OnUnitDied?.Invoke(this);
        }

        public void LookAtTarget(GridIndex targetIndex)
        {
            _tacticsGrid.GetTileDataFromIndex(targetIndex, out TileData tileData);
            Vector3 lookAtVector = tileData.tileMatrix.GetPosition();
            lookAtVector.y = this.transform.position.y;
            this.transform.LookAt(lookAtVector);
        }

        public void PlayAnimationType(AnimationType animationType)
        {
            _unitAnimator.SetTrigger(animationType.ToString());
        }

        public void PlayAttackAnimation()
        {
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
    }
}
