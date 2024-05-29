using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class ShowAbilityPatternAction : SelectTileAndUnitAction
    {
        [Header("Ability Range Pattern")]
        [SerializeField] private AbilityRangePattern _rangePattern = AbilityRangePattern.None;
        [SerializeField] private Vector2Int _rangeMinMax = new Vector2Int(0, 3);
        [SerializeField] private bool _rangeLineOfSight = false;
        [SerializeField] private float _rangeLineOfSightHeight = 0.5f;

        [Header("Area of Effect Pattern")]
        [SerializeField] private AbilityRangePattern _areaOfEffectPattern = AbilityRangePattern.None;
        [SerializeField] private Vector2Int _areaOfEffectRangeMinMax = new Vector2Int(0, 3);
        [SerializeField] private bool _areaOfEffectLineOfSight = false;
        [SerializeField] private float _areaOfEffectLoSHeight = 0.5f;

        public AbilityRangePattern RangePattern { get => _rangePattern; set => _rangePattern = value; }
        public AbilityRangePattern AreaOfEffectPattern { get => _areaOfEffectPattern; set => _areaOfEffectPattern = value; }
        public Vector2Int RangeMinMax { get => _rangeMinMax; set => _rangeMinMax = value; }
        public Vector2Int AreaOfEffectRangeMinMax { get => _areaOfEffectRangeMinMax; set => _areaOfEffectRangeMinMax = value; }
        public bool RangeLineOfSight { get => _rangeLineOfSight; set => _rangeLineOfSight = value; }
        public bool AreaOfEffectRequireLoS { get => _areaOfEffectLineOfSight; set => _areaOfEffectLineOfSight = value; }
        public float RangeLineOfSightHeight { get => _rangeLineOfSightHeight; set => _rangeLineOfSightHeight = value; }
        public float AreaOfEffectLoSHeight { get => _areaOfEffectLoSHeight; set => _areaOfEffectLoSHeight = value; }

        private List<GridIndex> _rangeIndexes = new List<GridIndex>();
        private List<GridIndex> _areaOfEffectIndexes = new List<GridIndex>();

        private GridIndex _selectedTileIndex = GridIndex.Invalid();
        private GridIndex _hoveredTileIndex = GridIndex.Invalid();

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_selectedTileIndex != index)
                _selectedTileIndex = index;
            else
                _selectedTileIndex = GridIndex.Invalid();


            ShowAbilityRangePattern();
            ShowAbilityAreaOfEffectPattern();

            return true;
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            _hoveredTileIndex = hoveredIndex;
            ShowAbilityAreaOfEffectPattern();
        }

        public void ShowAbilityRangePattern()
        {
            if (_rangeIndexes.Count > 0)
            {
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
                _rangeIndexes.Clear();
            }

            if (_playerActions.TacticsGrid.IsIndexValid(_selectedTileIndex))
            {
                _rangeIndexes = AbilityStatics.GetIndexesFromPatternAndRange(_selectedTileIndex, _playerActions.TacticsGrid.GridShape, _rangeMinMax, _rangePattern);

                if (_rangeLineOfSight)
                    _rangeIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_selectedTileIndex, _rangeIndexes, _rangeLineOfSightHeight);

                _rangeIndexes.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAbilityRange));
            }
        }
        public void ShowAbilityAreaOfEffectPattern()
        {
            if (_areaOfEffectIndexes.Count > 0)
            {
                _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
                _areaOfEffectIndexes.Clear();
            }

            if (_selectedTileIndex == GridIndex.Invalid())
                return;

            if (!_rangeIndexes.Contains(_hoveredTileIndex))
                return;

            if (_playerActions.TacticsGrid.IsIndexValid(_hoveredTileIndex))
            {
                _areaOfEffectIndexes = AbilityStatics.GetIndexesFromPatternAndRange(_hoveredTileIndex, _playerActions.TacticsGrid.GridShape, _areaOfEffectRangeMinMax, _areaOfEffectPattern);
                if (_areaOfEffectLineOfSight)
                    _areaOfEffectIndexes = CombatManager.Instance.RemoveIndexesWithoutLineOfSight(_hoveredTileIndex, _areaOfEffectIndexes, _areaOfEffectLoSHeight);
                _areaOfEffectIndexes.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAoeRange));
            }
        }


        private void OnDestroy()
        {
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAbilityRange);
            _playerActions.TacticsGrid.ClearAllTilesWithState(TileState.IsInAoeRange);
        }
    }
}