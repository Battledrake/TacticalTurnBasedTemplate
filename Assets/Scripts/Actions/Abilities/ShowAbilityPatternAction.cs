using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class ShowAbilityPatternAction : SelectTileAndUnitAction
    {
        [SerializeField] private Vector2Int _rangeMinMax = new Vector2Int(0, 5);
        private List<GridIndex> _displayList = new List<GridIndex>();

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_displayList.Count > 0)
            {
                _displayList.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAbilityRange));
                _displayList.Clear();
            }

            _displayList = PatternStatics.GetIndexesFromPatternAndRange(index, _playerActions.TacticsGrid.GridShape, _rangeMinMax, (AbilityRangePattern)actionValue);
            _displayList.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAbilityRange));

            return true;
        }

        private void OnDestroy()
        {
            if (_displayList.Count > 0)
            {
                _displayList.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAbilityRange));
            }
        }
    }
}