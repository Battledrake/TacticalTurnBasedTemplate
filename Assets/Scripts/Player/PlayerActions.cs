using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class PlayerActions : MonoBehaviour
    {
        [SerializeField] private TacticsGrid _tacticsGrid;

        public TacticsGrid TacticsGrid { get => _tacticsGrid; }
        public Vector2Int HoveredTile { get => _hoveredTile; set => _selectedTile = value; }
        public Vector2Int SelectedTile { get => _selectedTile; set => _selectedTile = value; }
        public ActionBase LeftClickAction { get => _leftClickAction; }
        public ActionBase RightClickAction { get => _rightClickAction; }

        private Vector2Int _hoveredTile;
        private Vector2Int _selectedTile;

        private ActionBase _leftClickAction;
        private ActionBase _rightClickAction;

        private void Awake()
        {
        }

        private void Update()
        {
            UpdateHoveredTile();

            if (Input.GetMouseButtonDown(0))
            {
                if (_leftClickAction)
                    Debug.Log(_leftClickAction.ExecuteAction(_hoveredTile));
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (_rightClickAction)
                    Debug.Log(_leftClickAction.ExecuteAction(_hoveredTile));
            }
        }
        private void UpdateHoveredTile()
        {
            if (_tacticsGrid.GetTileIndexUnderCursor() != _hoveredTile)
            {
                _tacticsGrid.RemoveStateFromTile(_hoveredTile, TileState.Hovered);
                _hoveredTile = _tacticsGrid.GetTileIndexUnderCursor();
                _tacticsGrid.AddStateToTile(_hoveredTile, TileState.Hovered);
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
            if (leftClickAction != null && rightClickAction != null)
            {
                _leftClickAction = GameObject.Instantiate(leftClickAction);
                _leftClickAction.InitializeAction(this);
                _rightClickAction = GameObject.Instantiate(rightClickAction);
                _rightClickAction.InitializeAction(this);
            }
        }
    }
}
