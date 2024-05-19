using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class ShowAbilityPatternAction : SelectTileAndUnitAction
    {
        [SerializeField] private Vector2Int _rangeMinMax = new Vector2Int(0, 5);
        [SerializeField] private bool _requireLineOfSight = false;
        [SerializeField] private float _lineOfSightHeight = 0.5f;

        public Vector2Int RangeMinMax { get => _rangeMinMax; set => _rangeMinMax = value; }
        public bool RequireLineOfSight { get => _requireLineOfSight; set => _requireLineOfSight = value; }
        public float LineOfSightHeight { get => _lineOfSightHeight; set => _lineOfSightHeight = value; }

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

            if (_playerActions.TacticsGrid.IsIndexValid(_currentIndex))
            {
                _displayList = AbilityStatics.GetIndexesFromPatternAndRange(_currentIndex, _playerActions.TacticsGrid.GridShape, _rangeMinMax, (AbilityRangePattern)actionValue);
                if (_requireLineOfSight)
                    _displayList = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_currentIndex, _displayList, _lineOfSightHeight);
                _displayList.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAbilityRange));
            }
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