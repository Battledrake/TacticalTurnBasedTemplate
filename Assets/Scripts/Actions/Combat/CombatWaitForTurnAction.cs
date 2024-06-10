using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class CombatWaitForTurnAction : ActionBase
    {
        [SerializeField] private Color _moveRangeColor = Color.cyan;
        [SerializeField] private float _outlineHeight = 0.3f;
        [SerializeField] private float _distanceToEdge = 1.0f;
        [SerializeField] private float _outlineLength = 1.0f;
        private List<LineRenderer> _borrowedMoveRenders = new List<LineRenderer>();
        public override bool ExecuteAction(GridIndex index)
        {
            return false;
        }

        private void OnDisable()
        {
            LineRendererPool.Instance.ReturnInstances(_borrowedMoveRenders);
        }

        public override void ExecuteHoveredAction(GridIndex hoveredIndex)
        {
            if (_borrowedMoveRenders.Count > 0)
            {
                LineRendererPool.Instance.ReturnInstances(_borrowedMoveRenders);
                _borrowedMoveRenders.Clear();
            }

            _playerActions.TacticsGrid.GetTileDataFromIndex(hoveredIndex, out TileData tileData);
            if (tileData.unitOnTile)
                GenerateTilesInMoveRange(tileData.unitOnTile);
        }

        private void GenerateTilesInMoveRange(Unit unit)
        {
            if (unit != null)
            {
                PathParams pathParams = GridPathfinding.CreatePathParamsFromUnit(unit, unit.GetMoveRange());
                PathfindingResult pathResult = _playerActions.TacticsGrid.GridPathfinder.FindTilesInRange(unit.UnitGridIndex, pathParams);
                if (pathResult.Result != PathResult.SearchFail)
                {
                    _borrowedMoveRenders = LineRendererPool.Instance.BorrowInstances(pathResult.Edges.Count);
                    int edgeIndex = 0;

                    foreach (EdgeData edge in pathResult.Edges)
                    {
                        _borrowedMoveRenders[edgeIndex].positionCount = 2;
                        _borrowedMoveRenders[edgeIndex].startColor = _moveRangeColor;
                        _borrowedMoveRenders[edgeIndex].endColor = _moveRangeColor;
                        _borrowedMoveRenders[edgeIndex].startWidth = 0.15f;
                        _borrowedMoveRenders[edgeIndex].endWidth = 0.15f;

                        Vector3 sourcePosition = _playerActions.TacticsGrid.GetTilePositionFromIndex(edge.source);
                        GridIndex direction = edge.direction;

                        switch (_playerActions.TacticsGrid.GridShape)
                        {
                            case GridShape.Square:
                                float vertexX0 = CalculateSquareVertex(sourcePosition.x, direction.x, direction.z, 0);
                                float vertexX1 = CalculateSquareVertex(sourcePosition.x, direction.x, direction.z, 1);
                                float vertexZ0 = CalculateSquareVertex(sourcePosition.z, direction.z, direction.x, 0);
                                float vertexZ1 = CalculateSquareVertex(sourcePosition.z, direction.z, direction.x, 1);

                                _borrowedMoveRenders[edgeIndex].SetPosition(0, new Vector3(vertexX0, sourcePosition.y + _outlineHeight, vertexZ0));
                                _borrowedMoveRenders[edgeIndex].SetPosition(1, new Vector3(vertexX1, sourcePosition.y + _outlineHeight, vertexZ1));
                                break;

                            case GridShape.Hexagon:
                                _borrowedMoveRenders[edgeIndex].SetPosition(0, sourcePosition);
                                _borrowedMoveRenders[edgeIndex].SetPosition(1, sourcePosition);
                                break;

                            case GridShape.Triangle:
                                _borrowedMoveRenders[edgeIndex].SetPosition(0, sourcePosition);
                                _borrowedMoveRenders[edgeIndex].SetPosition(1, sourcePosition);
                                break;
                        }
                        edgeIndex++;
                    }
                }
            }
        }

        private float CalculateSquareVertex(float source, float directionToEdge, float directionAlongEdge, int vertexIndex)
        {
            float gridTileSize = _playerActions.TacticsGrid.TileSize.x;
            float toEdge = source + (directionToEdge * (gridTileSize * 0.5f) * _distanceToEdge);
            float toVertex = (float)directionAlongEdge * (gridTileSize * 0.5f) * _outlineLength;
            return toEdge + (vertexIndex == 0 ? -toVertex : toVertex);
        }
    }
}