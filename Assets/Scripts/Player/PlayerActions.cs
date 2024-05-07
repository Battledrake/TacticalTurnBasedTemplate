using BattleDrakeCreations.TTBTk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TTBTk
{
    public class PlayerActions : MonoBehaviour
    {
        [SerializeField] private TacticsGrid _tacticsGrid;

        private Vector2Int _hoveredTile;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _tacticsGrid.AddStateToTile(_hoveredTile, TileState.Selected);
            }
            if (Input.GetMouseButtonDown(1))
            {
                _tacticsGrid.RemoveStateFromTile(_hoveredTile, TileState.Selected);
            }

            UpdateTileUnderCursor();
        }
        private void UpdateTileUnderCursor()
        {
            if(_tacticsGrid.GetTileIndexUnderCursor() != _hoveredTile)
            {
                _tacticsGrid.RemoveStateFromTile(_hoveredTile, TileState.Hovered);
                _hoveredTile = _tacticsGrid.GetTileIndexUnderCursor();
                _tacticsGrid.AddStateToTile(_hoveredTile, TileState.Hovered);
            }
        }
    }
}
