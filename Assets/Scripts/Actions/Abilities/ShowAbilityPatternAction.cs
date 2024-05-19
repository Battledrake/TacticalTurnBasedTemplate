using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class ShowAbilityPatternAction : SelectTileAndUnitAction
    {
        [SerializeField] private Vector2Int _rangeMinMax = new Vector2Int(0, 5);

        public Vector2Int RangeMinMax { get => _rangeMinMax; set => _rangeMinMax = value; }

        private List<GridIndex> _displayList = new List<GridIndex>();

        private GridIndex _currentIndex = GridIndex.Invalid();

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            _currentIndex = index;

            ShowAbilityRangePattern();

            return true;
        }

        public void ShowAbilityRangePattern()
        {
            if (_displayList.Count > 0)
            {
                _displayList.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAbilityRange));
                _displayList.Clear();
            }

            _displayList = AbilityStatics.GetIndexesFromPatternAndRange(_currentIndex, _playerActions.TacticsGrid.GridShape, _rangeMinMax, (AbilityRangePattern)actionValue);
            _displayList.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAbilityRange));
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