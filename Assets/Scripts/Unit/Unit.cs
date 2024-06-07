using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [RequireComponent(typeof(GridMovement), typeof(HealthVisual), typeof(AbilitySystem))]
    public class Unit : MonoBehaviour, IPlayAnimation, IAbilitySystem, IHealthVisual
    {
        public static event Action<Unit, GridIndex> OnAnyUnitReachedNewTile;
        public static event Action<Unit> OnAnyUnitDied;
        public static event Action<Unit> OnAnyUnitHealthChanged;
        public event Action<bool> OnUnitHoveredChanged;
        public event Action<bool> OnUnitSelectedChanged;
        public event Action<Unit> OnUnitReachedDestination;
        public event Action<Unit> OnUnitStartedMovement;
        public event Action<Unit> OnUnitMovementStopped;
        public event Action<Unit, bool> OnUnitDied;
        public event Action<Unit> OnUnitRespawn;
        public event Action OnTeamIndexChanged;

        [SerializeField] private UnitId _unitDataId = UnitId.Ranger;
        [SerializeField] private Transform _lookAtTransform;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _selectedColor = Color.green;

        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public Transform LookAtTransform { get => _lookAtTransform; }
        public GridIndex UnitGridIndex { get => _gridIndex; set => _gridIndex = value; }
        public UnitData UnitData { get => _unitData; }
        public bool IsAlive { get => _isAlive; }
        public int TeamIndex { get => _teamIndex; }
        public int PreviousTeamIndex { get => _prevTeamIndex; }

        private GameObject _unitVisual;
        private UnitData _unitData;
        private GridIndex _gridIndex = GridIndex.Invalid();
        private int _teamIndex = -1;
        private int _prevTeamIndex = -1;
        private bool _isAlive = true;

        //Components
        private Animator _unitAnimator;
        private AnimationEventHandler _animEventHandler;
        private TacticsGrid _tacticsGrid;
        private Collider _collider;
        private GridMovement _gridMovement;
        private HealthVisual _healthVisual;
        private AbilitySystem _abilitySystem;

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
            _healthVisual = this.GetComponent<HealthVisual>();
            _abilitySystem = this.GetComponent<AbilitySystem>();
        }

        private void OnEnable()
        {
            _gridMovement.OnMovementStarted += GridMovement_OnMovementStarted;
            _gridMovement.OnMovementStopped += GridMovement_OnMovementStopped;
            _gridMovement.OnReachedNewTile += GridMovement_OnReachedNewTile;
            _gridMovement.OnReachedDestination += GridMovement_OnReachedDestination;
        }

        private void OnDisable()
        {
            _gridMovement.OnMovementStarted -= GridMovement_OnMovementStarted;
            _gridMovement.OnMovementStopped -= GridMovement_OnMovementStopped;
            _gridMovement.OnReachedNewTile -= GridMovement_OnReachedNewTile;
            _gridMovement.OnReachedDestination -= GridMovement_OnReachedDestination;
        }

        public int GetHealth() => _abilitySystem.GetAttributeCurrentValue(AttributeId.Health);
        public int GetMaxHealth() => _abilitySystem.GetAttributeCurrentValue(AttributeId.MaxHealth);

        public int GetMoveRange() => _abilitySystem.GetAttributeCurrentValue(AttributeId.MoveRange);
        public int GetAgility() => _abilitySystem.GetAttributeCurrentValue(AttributeId.Agility);
        public AnimationEventHandler GetAnimationEventHandler() => _animEventHandler;
        public AbilitySystem GetAbilitySystem() => _abilitySystem;

        //Used for initial team setting or permanent team changes.
        public void SetTeamIndex(int index)
        {
            _prevTeamIndex = _teamIndex;
            _teamIndex = index;
            OnTeamIndexChanged?.Invoke();
            _healthVisual.SetHealthUnitColor(CombatManager.Instance.GetTeamColor(index));
        }

        //Used for team swaps due to abilities and allow prevTeamIndex to be grabbed later.
        public void ChangeTeam(int teamIndex)
        {
            _teamIndex = teamIndex;
            OnTeamIndexChanged?.Invoke();
        }

        public void CombatStarted()
        {
        }

        public void CombatEnded()
        {
        }

        public void TurnStarted()
        {
            _abilitySystem.ResetActionPoints();
        }

        public void TurnEnded()
        {
            //What we doing here eh?
        }

        private void GridMovement_OnMovementStarted()
        {
            OnUnitStartedMovement?.Invoke(this);
            _unitAnimator.SetTrigger(AnimationType.Run.ToString());
        }

        private void GridMovement_OnMovementStopped()
        {
            OnUnitMovementStopped?.Invoke(this);
        }

        private void GridMovement_OnReachedNewTile(GridIndex index)
        {
            OnAnyUnitReachedNewTile?.Invoke(this, index);
        }

        private void GridMovement_OnReachedDestination()
        {
            OnUnitReachedDestination?.Invoke(this);
            _unitAnimator.SetTrigger(AnimationType.Idle.ToString());
        }

        public void SetUnitsGrid(TacticsGrid grid)
        {
            _tacticsGrid = grid;
            _gridMovement.SetPathingGrid(grid);
        }

        public void InitUnit(UnitId dataId)
        {
            _unitDataId = dataId;
            _unitData = DataManager.GetUnitDataFromId(_unitDataId);

            if (_unitVisual == null)
                _unitVisual = Instantiate(_unitData.assetData.unitVisual, this.transform);

            _isAlive = true;

            InitComponents();
        }

        private void InitComponents()
        {
            _healthVisual.InitHealthVisual(this);
            _abilitySystem.OnAttributeBaseChanged += AbilitySystem_OnAttributeBaseChanged;
            _abilitySystem.OnAttributeCurrentChanged += AbilitySystem_OnAttributeCurrentChanged;
            _abilitySystem.InitAbilitySystem(this, _unitData.unitStats.attributes, _unitData.unitStats.abilities);

            _unitAnimator = _unitVisual.GetComponent<Animator>();
            _unitOutline = _unitVisual.AddComponent<Outline>();
            _animEventHandler = _unitVisual.AddComponent<AnimationEventHandler>();

            Transform headTransform = FindTransform(_unitVisual, "Head");
            if (headTransform)
            {
                _healthVisual.HealthBar.position = headTransform.position + Vector3.up;
            }
        }

        private void AbilitySystem_OnAttributeCurrentChanged(AttributeId id, int oldValue, int newValue)
        {
            Debug.Log($"Attribute Current Changed: {id}, Old: {oldValue}, New: {newValue}");

            if (id == AttributeId.Health)
            {
                OnAnyUnitHealthChanged?.Invoke(this);
                _healthVisual.DisplayHealthChange(newValue - oldValue);

                if(newValue <= 0)
                {
                    Die();
                }
                else
                {
                    if(newValue - oldValue < 0)
                    {
                        PlayAnimationType(AnimationType.Hit);
                    }
                }
            }

            if (id == AttributeId.MaxHealth)
            {
                _healthVisual.UpdateHealthVisual();
            }
        }

        private void AbilitySystem_OnAttributeBaseChanged(AttributeId id, int oldValue, int newValue)
        {
            Debug.Log($"Attribute Base Changed: {id}, Old: {oldValue}, New: {newValue}");
        }

        private Transform FindTransform(GameObject parentObject, string transformName)
        {
            return parentObject.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == transformName);
        }

        [ContextMenu("ChangeUnit")]
        public void ChangeUnitId()
        {
            InitUnit(_unitDataId);
        }

        [ContextMenu("ResetUnit")]
        public void ResetUnit()
        {
            if (!_isAlive)
            {
                _isAlive = true;

                CombatManager.Instance.AddUnitToCombat(this.transform.position, this, _prevTeamIndex);

                _unitAnimator.ResetTrigger(AnimationType.Hit.ToString());
                _unitAnimator.SetTrigger(AnimationType.Respawn.ToString());
                _collider.enabled = true;
            }
            _abilitySystem.InitAbilitySystem(this, _unitData.unitStats.attributes, _unitData.unitStats.abilities);
            //_healthVisual.InitHealthVisual(this);
            //_healthVisual.UpdateHealthVisual(_abilitySystem.GetAttributeBaseValue(AttributeId.MaxHealth));
        }

        public void SetIsHovered(bool isHovered)
        {
            _isHovered = isHovered;
            UpdateOutlineVisual();
            OnUnitHoveredChanged?.Invoke(_isHovered);
        }

        public void SetIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            UpdateOutlineVisual();
            OnUnitSelectedChanged?.Invoke(_isSelected);
        }

        public void UpdateOutlineVisual()
        {
            if (!_unitOutline)
                return;

            if (!_isHovered)
            {
                _unitOutline.enabled = false;
                return;
            }
            _unitOutline.enabled = true;

            _unitOutline.OutlineColor = CombatManager.Instance.GetTeamColor(_teamIndex);
            _unitOutline.OutlineWidth = _defaultOutlineWidth;
        }

        public void Die(bool shouldDestroy = false)
        {
            _isAlive = false;
            _gridMovement.Stop();
            _collider.enabled = false;
            PlayAnimationType(AnimationType.Death);
            OnUnitDied?.Invoke(this, shouldDestroy);
            OnAnyUnitDied?.Invoke(this);
        }

        public void LookAtTarget(GridIndex targetIndex)
        {
            Vector3 lookAtVector = _tacticsGrid.GetTilePositionFromIndex(targetIndex);
            lookAtVector.y = this.transform.position.y;
            this.transform.LookAt(lookAtVector);
        }

        public void PlayAnimationType(AnimationType animationType)
        {
            _unitAnimator.SetTrigger(animationType.ToString());
        }
    }
}
