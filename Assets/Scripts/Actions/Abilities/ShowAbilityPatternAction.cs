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

        public override void InitializeAction(PlayerActions playerActions)
        {
            base.InitializeAction(playerActions);

            _playerActions.OnHoveredTileChanged += PlayerActions_OnHoveredTileChanged;
        }

        private void PlayerActions_OnHoveredTileChanged(GridIndex index)
        {
            _hoveredTileIndex = index;
            ShowAbilityAreaOfEffectPattern();
        }

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

        public void ShowAbilityRangePattern()
        {
            if (_rangeIndexes.Count > 0)
            {
                _rangeIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAbilityRange));
                _rangeIndexes.Clear();
            }

            if (_playerActions.TacticsGrid.IsIndexValid(_selectedTileIndex))
            {
                if (_rangePattern != AbilityRangePattern.Movement)
                {
                    _rangeIndexes = AbilityStatics.GetIndexesFromPatternAndRange(_selectedTileIndex, _playerActions.TacticsGrid.GridShape, _rangeMinMax, _rangePattern);
                }
                else
                {
                    PathFilter pathFilter = _playerActions.TacticsGrid.GridPathfinder.CreateDefaultPathFilter(_rangeMinMax.y);
                    _rangeIndexes = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(_selectedTileIndex, pathFilter).Path;
                }

                if (_rangeLineOfSight)
                    _rangeIndexes = CombatSystem.Instance.RemoveIndexesWithoutLineOfSight(_selectedTileIndex, _rangeIndexes, _rangeLineOfSightHeight);

                _rangeIndexes.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAbilityRange));
            }
        }
        public void ShowAbilityAreaOfEffectPattern()
        {
            if (_areaOfEffectIndexes.Count > 0)
            {
                _areaOfEffectIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAoeRange));
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
                    _areaOfEffectIndexes = CombatSystem.Instance.RemoveIndexesWithoutLineOfSight(_hoveredTileIndex, _areaOfEffectIndexes, _areaOfEffectLoSHeight);
                _areaOfEffectIndexes.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInAoeRange));
            }
        }


        private void OnDestroy()
        {
            _playerActions.OnHoveredTileChanged -= PlayerActions_OnHoveredTileChanged;

            if (_rangeIndexes.Count > 0)
                _rangeIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAbilityRange));
            if (_areaOfEffectIndexes.Count > 0)
                _areaOfEffectIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInAoeRange));
        }
    }
}