using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class PlayerActions : MonoBehaviour
    {
        public event Action<ActionBase, ActionBase> OnSelectedActionsChanged;
        public event Action<GridIndex> OnHoveredTileChanged;
        public event Action<Ability> OnCurrentAbilityChanged;
        public event Action<GridIndex> OnSelectedTileChanged;
        public event Action<Unit> OnSelectedUnitChanged;

        [SerializeField] private Button _endCombatButton;
        [SerializeField] private Button _endTurnButton;

        [Header("Actions")]
        [SerializeField] private CombatMoveAction _combatMoveActionPrefab;

        [Header("Dependencies")]
        [SerializeField] private TacticsGrid _tacticsGrid;
        [SerializeField] private AbilityTabController _abilityTabController;

        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public GridIndex HoveredTile { get => _hoveredTile; set => _selectedTile = value; }
        public GridIndex SelectedTile { get => _selectedTile; set => _selectedTile = value; }
        public Unit HoveredUnit { get => _hoveredUnit; set => _hoveredUnit = value; }
        public Unit SelectedUnit { get => _selectedUnit; }
        public ActionBase LeftClickAction { get => _leftClickAction; }
        public ActionBase RightClickAction { get => _rightClickAction; }
        public Ability CurrentAbility { get => _currentAbility; set { _currentAbility = value; OnCurrentAbilityChanged?.Invoke(_currentAbility); } }

        private GridIndex _hoveredTile = new GridIndex(int.MinValue, int.MinValue);
        private GridIndex _selectedTile = new GridIndex(int.MinValue, int.MinValue);
        private Unit _hoveredUnit = null;
        private Unit _selectedUnit = null;

        private ActionBase _leftClickAction;
        private ActionBase _rightClickAction;

        private Ability _currentAbility = null;

        private bool _isLeftClickDown = false;
        private bool _isRightClickDown = false;


        private void Awake()
        {
            OnHoveredTileChanged += OnHoveredTileChanged_UpdateActions;
            OnSelectedUnitChanged += This_OnSelectedUnitChanged;
        }

        private void This_OnSelectedUnitChanged(Unit unit)
        {
            if (!_abilityTabController.ActiveAbility)
            {
                _currentAbility = null;
                OnCurrentAbilityChanged?.Invoke(null);
            }
        }

        private void Start()
        {
            CombatManager.Instance.OnUnitGridIndexChanged += CombatManager_OnUnitGridIndexChanged;
            Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
            CombatManager.Instance.OnCombatStarted += CombatManager_OnCombatStarted;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
            CombatManager.Instance.OnUnitTurnStarted += CombatManager_OnUnitTurnStarted;
            CombatManager.Instance.OnUnitTurnEnded += CombatManager_OnUnitTurnEnded;
        }

        private void CombatManager_OnCombatStarted()
        {
            _endCombatButton.gameObject.SetActive(true);
            ClearSelectedActions();
            SetSelectedTileAndUnit(GridIndex.Invalid());
        }

        private void CombatManager_OnCombatEnded()
        {
            _endCombatButton.gameObject.SetActive(false);
            _endTurnButton.gameObject.SetActive(false);
            ClearSelectedActions();
            SetSelectedTileAndUnit(GridIndex.Invalid());
        }

        private void CombatManager_OnUnitTurnStarted(Unit unit)
        {
            _endTurnButton.gameObject.SetActive(true);
            SetSelectedTileAndUnit(unit.UnitGridIndex);
            SetSelectedActions(_combatMoveActionPrefab, null);
        }

        private void CombatManager_OnUnitTurnEnded(Unit unit)
        {
            _endTurnButton.gameObject.SetActive(false);
            ClearSelectedActions();
        }

        private void Unit_OnAnyUnitDied(Unit unit)
        {
            if (unit == _selectedUnit)
            {
                _selectedUnit = null;
                OnSelectedUnitChanged.Invoke(null);
            }
        }

        private void CombatManager_OnUnitGridIndexChanged(Unit unit, GridIndex index)
        {
            if (_selectedUnit == unit)
                SetSelectedTileAndUnit(index);
        }

        private void OnHoveredTileChanged_UpdateActions(GridIndex gridIndex)
        {
            if (_isLeftClickDown)
            {
                TryLeftClickAction();
            }
            if (_isRightClickDown)
            {
                TryRightClickAction();
            }
        }

        private void Update()
        {
            UpdatedHoveredTileAndUnit();

            if (Input.GetMouseButtonDown(0))
            {
                _isLeftClickDown = true;
                TryLeftClickAction();
            }
            if (Input.GetMouseButtonUp(0))
            {
                _isLeftClickDown = false;
            }
            if (Input.GetMouseButtonDown(1))
            {
                _isRightClickDown = true;
                TryRightClickAction();
            }
            if (Input.GetMouseButtonUp(1))
            {
                _isRightClickDown = false;
            }


        }

        private void TryLeftClickAction()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (_leftClickAction)
                _leftClickAction.ExecuteAction(_hoveredTile);
        }
        private void TryRightClickAction()
        {
            if (_rightClickAction)
                _rightClickAction.ExecuteAction(_hoveredTile);
        }

        private void UpdatedHoveredTileAndUnit()
        {
            Unit unit = GetUnitUnderCursor();
            if (_hoveredUnit != unit)
            {
                if (_hoveredUnit != null)
                    _hoveredUnit.SetIsHovered(false);

                if (unit != null)
                    unit.SetIsHovered(true);

                _hoveredUnit = unit;
            }

            GridIndex newIndex;
            if (_hoveredUnit)
            {
                newIndex = _hoveredUnit.UnitGridIndex;
            }
            else
            {
                newIndex = _tacticsGrid.GetTileIndexUnderCursor();
            }

            if (newIndex != _hoveredTile)
            {
                _tacticsGrid.RemoveStateFromTile(_hoveredTile, TileState.Hovered);
                _hoveredTile = newIndex;
                _tacticsGrid.AddStateToTile(_hoveredTile, TileState.Hovered);
                OnHoveredTileChanged?.Invoke(_hoveredTile);
            }
        }

        public void SetSelectedTileAndUnit(GridIndex index)
        {
            GridIndex previousTile = _selectedTile;
            if (previousTile != index)
            {
                _tacticsGrid.RemoveStateFromTile(previousTile, TileState.Selected);
                _selectedTile = index;
                OnSelectedTileChanged?.Invoke(_selectedTile);
                _tacticsGrid.AddStateToTile(index, TileState.Selected);
            }
            else //Clicked on a tile that was already selected
            {
                _tacticsGrid.RemoveStateFromTile(index, TileState.Selected);
                _selectedTile = GridIndex.Invalid();
                OnSelectedTileChanged?.Invoke(_selectedTile);
                if (_selectedUnit != null)
                {
                    _selectedUnit.SetIsSelected(false);
                    _selectedUnit = null;
                    OnSelectedUnitChanged?.Invoke(null);
                    return;
                }
            }

            _tacticsGrid.GridTiles.TryGetValue(index, out TileData tileData);

            if (tileData.unitOnTile != _selectedUnit)
            {
                if (_selectedUnit != null)
                {
                    _selectedUnit.SetIsSelected(false);
                }
                if (tileData.unitOnTile != null)
                {
                    tileData.unitOnTile.SetIsSelected(true);
                }
                _selectedUnit = tileData.unitOnTile;
                OnSelectedUnitChanged?.Invoke(_selectedUnit);
            }
        }

        public void SetLeftClickActionValue(int value)
        {
            if (_leftClickAction != null)
                _leftClickAction.actionValue = value;
        }

        public void SetRightClickActionValue(int value)
        {
            if (_rightClickAction != null)
                _rightClickAction.actionValue = value;
        }

        public void ClearSelectedActions()
        {
            if (_leftClickAction != null)
            {
                Destroy(_leftClickAction.gameObject);
                _leftClickAction = null;
            }

            if (_rightClickAction != null)
            {
                Destroy(_rightClickAction.gameObject);
                _rightClickAction = null;
            }
        }

        public void SetSelectedActions(ActionBase leftClickAction, ActionBase rightClickAction)
        {
            ClearSelectedActions();

            _leftClickAction = Instantiate(leftClickAction);
            _leftClickAction.InitializeAction(this);

            if (rightClickAction != null)
            {
                _rightClickAction = Instantiate(rightClickAction);
                _rightClickAction.InitializeAction(this);
            }

            OnSelectedActionsChanged?.Invoke(_leftClickAction, _rightClickAction);
        }

        public Unit GetUnitUnderCursor()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            LayerMask unitLayer = LayerMask.GetMask("Unit");

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, unitLayer))
            {
                return hitInfo.transform.GetComponent<Unit>();
            }
            else
            {
                GridIndex tileIndex = _tacticsGrid.GetTileIndexUnderCursor();
                _tacticsGrid.GridTiles.TryGetValue(tileIndex, out TileData tileData);

                return tileData.unitOnTile;
            }
        }
    }
}
