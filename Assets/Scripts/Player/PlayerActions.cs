using System;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class PlayerActions : MonoBehaviour
    {
        public event Action<ActionBase, ActionBase> SelectedActionsChanged;

        [SerializeField] private TacticsGrid _tacticsGrid;

        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public GridIndex HoveredTile { get => _hoveredTile; set => _selectedTile = value; }
        public GridIndex SelectedTile { get => _selectedTile; set => _selectedTile = value; }
        public ActionBase LeftClickAction { get => _leftClickAction; }
        public ActionBase RightClickAction { get => _rightClickAction; }

        private GridIndex _hoveredTile;
        private GridIndex _selectedTile;

        private ActionBase _leftClickAction;
        private ActionBase _rightClickAction;

        private Action HoverTileChanged;

        private bool _isLeftClickDown = false;
        private bool _isRightClickDown = false;


        private void Awake()
        {
            HoverTileChanged += OnHoverTileChanged;
        }

        private void OnHoverTileChanged()
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
            UpdateHoveredTile();

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
            if (_leftClickAction)
                _leftClickAction.ExecuteAction(_hoveredTile);
        }
        private void TryRightClickAction()
        {
            if (_rightClickAction)
                _rightClickAction.ExecuteAction(_hoveredTile);
        }

        private void UpdateHoveredTile()
        {
            if (_tacticsGrid.GetTileIndexUnderCursor() != _hoveredTile)
            {
                _tacticsGrid.RemoveStateFromTile(_hoveredTile, TileState.Hovered);
                _hoveredTile = _tacticsGrid.GetTileIndexUnderCursor();
                _tacticsGrid.AddStateToTile(_hoveredTile, TileState.Hovered);
                HoverTileChanged?.Invoke();
            }
        }

        public void ClearSelectedActions()
        {
            Destroy(_leftClickAction.gameObject);
            _leftClickAction = null;
            Destroy(_rightClickAction.gameObject);
            _rightClickAction = null;
        }

        public void SetSelectedActions(ActionBase leftClickAction, ActionBase rightClickAction)
        {
            if (_leftClickAction != null)
                ClearSelectedActions();

            _leftClickAction = GameObject.Instantiate(leftClickAction);
            _leftClickAction.InitializeAction(this);
            _rightClickAction = GameObject.Instantiate(rightClickAction);
            _rightClickAction.InitializeAction(this);

            SelectedActionsChanged?.Invoke(_leftClickAction, _rightClickAction);
        }
    }
}
