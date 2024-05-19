using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{

    public class ShowAbilityPatternAction : SelectTileAndUnitAction
    {
        [Header("To Target Pattern")]
        [SerializeField] private AbilityRangePattern _toTargetPattern = AbilityRangePattern.None;
        [SerializeField] private Vector2Int _toTargetRangeMinMax = new Vector2Int(0, 3);
        [SerializeField] private bool _toTargetLineOfSight = false;
        [SerializeField] private float _toTargetLineOfSightHeight = 0.5f;

        [Header("On Target Pattern")]
        [SerializeField] private AbilityRangePattern _onTargetPattern = AbilityRangePattern.None;
        [SerializeField] private Vector2Int _onTargetRangeMinMax = new Vector2Int(0, 3);
        [SerializeField] private bool _onTargetLineOfSight = false;
        [SerializeField] private float _onTargetLineOfSightHeight = 0.5f;

        public AbilityRangePattern AbilityToTargetPattern { get => _toTargetPattern; set => _toTargetPattern = value; }
        public AbilityRangePattern AbilityOnTargetPattern { get => _onTargetPattern; set => _onTargetPattern = value; }
        public Vector2Int ToTargetRangeMinMax { get => _toTargetRangeMinMax; set => _toTargetRangeMinMax = value; }
        public Vector2Int OnTargetRangeMinMax { get => _onTargetRangeMinMax; set => _onTargetRangeMinMax = value; }
        public bool ToTargetRequireLineOfSight { get => _toTargetLineOfSight; set => _toTargetLineOfSight = value; }
        public bool OnTargetRequireLineOfSight { get => _onTargetLineOfSight; set => _onTargetLineOfSight = value; }
        public float ToTargetLineOfSightHeight { get => _toTargetLineOfSightHeight; set => _toTargetLineOfSightHeight = value; }
        public float OnTargetLineOfSightHeight { get => _onTargetLineOfSightHeight; set => _onTargetLineOfSightHeight = value; }

        private List<GridIndex> _toTargetIndexes = new List<GridIndex>();
        private List<GridIndex> _onTargetIndexes = new List<GridIndex>();

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
            ShowAbilityOnTargetRangePattern();
        }

        public override bool ExecuteAction(GridIndex index)
        {
            base.ExecuteAction(index);

            if (_selectedTileIndex != index)
                _selectedTileIndex = index;
            else
                _selectedTileIndex = GridIndex.Invalid();


            ShowAbilityToTargetRangePattern();
            ShowAbilityOnTargetRangePattern();

            return true;
        }

        public void ShowAbilityToTargetRangePattern()
        {
            if (_toTargetIndexes.Count > 0)
            {
                _toTargetIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInToTargetRange));
                _toTargetIndexes.Clear();
            }

            if (_playerActions.TacticsGrid.IsIndexValid(_selectedTileIndex))
            {
                _toTargetIndexes = AbilityStatics.GetIndexesFromPatternAndRange(_selectedTileIndex, _playerActions.TacticsGrid.GridShape, _toTargetRangeMinMax, _toTargetPattern);
                if (_toTargetLineOfSight)
                    _toTargetIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_selectedTileIndex, _toTargetIndexes, _toTargetLineOfSightHeight);
                _toTargetIndexes.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInToTargetRange));
            }
        }
        public void ShowAbilityOnTargetRangePattern()
        {
            if (_onTargetIndexes.Count > 0)
            {
                _onTargetIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInOnTargetRange));
                _onTargetIndexes.Clear();
            }

            if (_selectedTileIndex == GridIndex.Invalid())
                return;

            if (!_toTargetIndexes.Contains(_hoveredTileIndex))
                return;

            if (_playerActions.TacticsGrid.IsIndexValid(_hoveredTileIndex))
            {
                _onTargetIndexes = AbilityStatics.GetIndexesFromPatternAndRange(_hoveredTileIndex, _playerActions.TacticsGrid.GridShape, _onTargetRangeMinMax, _onTargetPattern);
                if (_onTargetLineOfSight)
                    _onTargetIndexes = _playerActions.CombatSystem.RemoveIndexesWithoutLineOfSight(_hoveredTileIndex, _onTargetIndexes, _onTargetLineOfSightHeight);
                _onTargetIndexes.ForEach(i => _playerActions.TacticsGrid.AddStateToTile(i, TileState.IsInOnTargetRange));
            }
        }


        private void OnDestroy()
        {
            _playerActions.OnHoveredTileChanged -= PlayerActions_OnHoveredTileChanged;

            if (_toTargetIndexes.Count > 0)
                _toTargetIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInToTargetRange));
            if (_onTargetIndexes.Count > 0)
                _onTargetIndexes.ForEach(i => _playerActions.TacticsGrid.RemoveStateFromTile(i, TileState.IsInOnTargetRange));
        }
    }
}