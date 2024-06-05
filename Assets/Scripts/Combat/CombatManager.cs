using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public enum TurnOrderType
    {
        Team,
        Stat,
        Random,
        FIFO
    }

    [Serializable]
    public struct TeamColorData
    {
        public int index;
        public Color color;
    }

    public struct CombatStartParams
    {
        public bool canStartCombat;
        public bool hasEnoughUnits;
        public bool hasEnoughTeams;
        public bool isNotInCombat;
    }

    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance;

        public event Action<Unit, GridIndex> OnUnitGridIndexChanged;
        public event Action OnUnitTeamChanged;
        public event Action OnCombatStarted;
        public event Action OnCombatEnded;
        public event Action<Unit> OnUnitTurnStarted;
        public event Action<Unit> OnUnitTurnEnded;
        public event Action OnAbilityUseCompleted;
        public event Action OnActiveTeamChanged;
        public event Action<Unit> OnActiveUnitChanged;
        public event Action<Unit> OnUnitAddedDuringCombat;
        public event Action<int> OnCombatFinishing;

        [SerializeField] TurnOrderType _turnOrderType;
        [SerializeField] private List<TeamColorData> _teamColors;

        [Header("Dependencies")]
        [SerializeField] private TacticsGrid _tacticsGrid;

        public TurnOrderType TurnOrderType { get => _turnOrderType; set => _turnOrderType = value; }
        public List<Unit> UnitsInCombat { get => _unitsInCombat; }
        public Dictionary<int, HashSet<Unit>> UnitTeams { get => _unitTeams; }
        public List<Unit> OrderedUnits { get => _orderedUnits; }
        public int NumberOfTeams { get => _teamColors.Count; }
        public bool IsInCombat { get => _isInCombat; }
        public bool ShowEnemyMoveRange { get => _showEnemyMoveRange; set => _showEnemyMoveRange = value; }
        public Unit ActiveUnit { get => _activeUnit; }

        private List<Unit> _unitsInCombat = new List<Unit>();
        private Dictionary<int, HashSet<Unit>> _unitTeams = new Dictionary<int, HashSet<Unit>>();
        private List<Unit> _orderedUnits = new List<Unit>();

        private int _turnsCompleted = 0;

        private bool _isInCombat = false;
        private bool _isCombatFinishing = false;
        private bool _showEnemyMoveRange = false;

        private Unit _activeUnit = null;
        private int _activeTeamIndex = -1;
        private int _actionPointDeduction = 0;
        private int _playerControlledIndex = 100;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        public Color GetTeamColor(int teamIndex)
        {
            return _teamColors.FirstOrDefault(d => d.index == teamIndex).color;
        }

        private void OnEnable()
        {
            _tacticsGrid.OnGridGenerated += TacticsGrid_OnGridGenerated;
            _tacticsGrid.OnTileDataUpdated += TacticsGrid_OnTileDataUpdated;
            _tacticsGrid.OnTileHeightChanged += TacticsGrid_OnTileHeightChanged;
        }

        private void OnDisable()
        {
            _tacticsGrid.OnGridGenerated -= TacticsGrid_OnGridGenerated;
            _tacticsGrid.OnTileDataUpdated -= TacticsGrid_OnTileDataUpdated;
            _tacticsGrid.OnTileHeightChanged -= TacticsGrid_OnTileHeightChanged;
        }
        public CombatStartParams CanStartCombat()
        {
            CombatStartParams combatStartParams;
            combatStartParams.isNotInCombat = !_isInCombat;
            combatStartParams.hasEnoughTeams = _unitTeams.Count(kvp => kvp.Value.Count > 0) >= 2;
            combatStartParams.hasEnoughUnits = _unitsInCombat.Count >= 2;
            combatStartParams.canStartCombat = !_isInCombat && _unitTeams.Count(kvp => kvp.Value.Count > 0) >= 2 && _unitsInCombat.Count >= 2;

            return combatStartParams;
        }

        private void SetActiveTeamIndex()
        {
            var teamIndexes = _unitTeams.Keys.Where(k => k > _activeTeamIndex && _unitTeams[k]?.Count > 0).OrderBy(k => k);
            if (teamIndexes.Any())
            {
                _activeTeamIndex = teamIndexes.First();
            }
            else
            {
                _activeTeamIndex = _unitTeams.Keys.Min();
            }
        }

        private void OrderUnitsByTeam()
        {
            if (_unitTeams.TryGetValue(_activeTeamIndex, out HashSet<Unit> teamUnits))
            {
                if (teamUnits != null && teamUnits.Count > 0)
                {
                    foreach (var unit in teamUnits)
                    {
                        _orderedUnits.Add(unit);
                    }
                }
            }
        }

        private void OrderUnitsByStat()
        {
            List<Unit> unitsToSort = new List<Unit>(_unitsInCombat);
            unitsToSort.Sort((unit1, unit2) =>
            {
                if (unit1.Agility > unit2.Agility)
                    return -1;
                else if (unit1.Agility < unit2.Agility)
                    return 1;
                else
                    return 0;
            });
            for (int i = 0; i < unitsToSort.Count; i++)
            {
                _orderedUnits.Add(unitsToSort[i]);
            }
        }

        private void OrderUnitsRandomly()
        {
            int listCount = _unitsInCombat.Count;
            List<Unit> unitsToRandomize = new List<Unit>(_unitsInCombat);
            for (int i = 0; i < listCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, unitsToRandomize.Count);
                _orderedUnits.Add(unitsToRandomize[randomIndex]);
                unitsToRandomize.Remove(unitsToRandomize[randomIndex]);
            }
        }

        private void OrderUnitsByFIFO()
        {
            for (int i = 0; i < _unitsInCombat.Count; i++)
            {
                _orderedUnits.Add(_unitsInCombat[i]);
            }
        }

        private void OrderUnitsByTurnOrderType()
        {
            switch (_turnOrderType)
            {
                case TurnOrderType.Team:
                    SetActiveTeamIndex();
                    OrderUnitsByTeam();
                    break;
                case TurnOrderType.Stat:
                    OrderUnitsByStat();
                    break;
                case TurnOrderType.Random:
                    OrderUnitsRandomly();
                    break;
                case TurnOrderType.FIFO:
                    OrderUnitsByFIFO();
                    break;
            }
        }

        public void StartCombat()
        {
            _activeTeamIndex = -1;
            _orderedUnits.Clear();

            OrderUnitsByTurnOrderType();

            for (int i = 0; i < _unitsInCombat.Count; i++)
            {
                _unitsInCombat[i].CombatStarted();
            }

            _isInCombat = true;
            _isCombatFinishing = false;
            OnCombatStarted?.Invoke();

            _turnsCompleted = 0;

            _activeUnit = null;

            StartTurn();
        }

        private void StartTurn()
        {
            if (_turnOrderType == TurnOrderType.Team)
            {
                if (_activeTeamIndex == _playerControlledIndex)
                {
                    //AI LOGIC IN HUR
                }
                else
                {
                    AdvanceToNextUnit();

                    for (int i = 0; i < _orderedUnits.Count; i++)
                    {
                        _orderedUnits[i].TurnStarted();
                    }
                }
            }
            else
            {
                AdvanceToNextUnit();
                _activeUnit.TurnStarted();
            }

            OnActiveUnitChanged?.Invoke(_activeUnit);

            //TODO: Make controller a singleton? Or different way of handling.
            GameObject.Find("[Cameras]").GetComponent<CameraController>().SetMoveToTarget(_activeUnit.transform.position);
        }

        private void AdvanceToNextUnit()
        {
            int activeIndex = _orderedUnits.IndexOf(_activeUnit);
            _activeUnit = _orderedUnits[++activeIndex % _orderedUnits.Count];
        }

        private void NextTurn()
        {
            if (_turnOrderType == TurnOrderType.Team)
            {
                _turnsCompleted = 0;
                _orderedUnits.Clear();
                SetActiveTeamIndex();
                OrderUnitsByTeam();
                OnActiveTeamChanged?.Invoke();
            }
            //Temporary as starting next turns too fast is a terrible experience.
            StartCoroutine(WaitToStartTurn());
            IEnumerator WaitToStartTurn()
            {
                yield return new WaitForSeconds(1f);
                StartTurn();
            }
        }

        public void EndUnitTurn()
        {
            OnUnitTurnEnded?.Invoke(_activeUnit);
            _turnsCompleted++;
            if (_turnOrderType == TurnOrderType.Team)
            {
                if (_turnsCompleted >= _unitTeams[_activeTeamIndex].Count)
                {
                    NextTurn();
                }
                else
                {
                    Unit turnEndedUnit = _activeUnit;
                    CycleUnits();
                    _orderedUnits.Remove(turnEndedUnit);
                }
            }
            else
            {
                NextTurn();
            }
        }

        //If null is passed in, grab next in list. Otherwise, select unit passed in selected.
        public void CycleUnits(Unit unit = null)
        {
            Unit currentUnit = _activeUnit;
            if (_turnOrderType == TurnOrderType.Team)
            {
                if (_orderedUnits.Contains(unit))
                {
                    _activeUnit = unit;
                }
                else
                {
                    AdvanceToNextUnit();
                }
                if (currentUnit != _activeUnit)
                    OnActiveUnitChanged?.Invoke(_activeUnit);
            }
        }

        public void EndCombat()
        {
            for (int i = 0; i < _unitsInCombat.Count; i++)
            {
                _unitsInCombat[i].CombatEnded();
            }

            _activeUnit = null;
            _isInCombat = false;
            OnCombatEnded?.Invoke();
        }

        private void FinishCombat(int winTeamIndex)
        {
            _isCombatFinishing = true;
            OnCombatFinishing?.Invoke(winTeamIndex);
        }

        public void MoveUnit(Unit unit, List<GridIndex> path, float pathLength)
        {
            GridMovement gridMoveComp = unit.GetComponent<GridMovement>();
            if (gridMoveComp)
            {
                unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                gridMoveComp.SetPathAndMove(path);
            }

            _actionPointDeduction = pathLength <= unit.MoveRange ? 1 : 2;

            unit.GetAbilitySystem().RemoveActionPoints(_actionPointDeduction);

            _actionPointDeduction = 0;

            _tacticsGrid.RemoveUnitFromTile(unit.UnitGridIndex);
            _tacticsGrid.AddUnitToTile(path.Last(), unit, false);
            OnUnitGridIndexChanged?.Invoke(unit, path.Last());

            if (unit.GetAbilitySystem().CurrentActionPoints <= 0)
                EndUnitTurn();
        }

        public void TeleportUnit(Unit unit, GridIndex targetIndex)
        {
            _tacticsGrid.RemoveUnitFromTile(unit.UnitGridIndex);
            _tacticsGrid.AddUnitToTile(targetIndex, unit, true, true);
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;
        }

        public void AddUnitToCombat(Vector3 worldPosition, Unit unit, int teamIndex = 0)
        {
            GridIndex unitIndex = _tacticsGrid.GetTileIndexFromWorldPosition(worldPosition);
            AddUnitToCombat(unitIndex, unit, teamIndex);
        }

        public void AddUnitToCombat(GridIndex gridIndex, Unit unit, int teamIndex = 0)
        {
            if (_tacticsGrid.AddUnitToTile(gridIndex, unit))
            {
                _unitsInCombat.Add(unit);
                unit.OnUnitDied += Unit_OnUnitDied;
            }
            else
            {
                Debug.LogWarning("Unable to add unit to tile. Invalid index or unit already exists at GridIndex");
            }
            SetUnitTeamIndex(unit, teamIndex);

            if (_isInCombat)
            {
                if (_turnOrderType == TurnOrderType.Team)
                {
                    if (teamIndex == _activeTeamIndex)
                        _orderedUnits.Add(unit);
                    else
                        return;
                }
                else
                {
                    int activeIndex = _orderedUnits.IndexOf(_activeUnit);
                    _orderedUnits.Insert(++activeIndex, unit);
                }
                OnUnitAddedDuringCombat?.Invoke(unit);
            }
        }

        public void SetUnitTeamIndex(Unit unit, int teamIndex)
        {
            int previousIndex = unit.TeamIndex;
            if (previousIndex != -1)
            {
                _unitTeams.TryGetValue(previousIndex, out HashSet<Unit> unitsInTeam);
                if (unitsInTeam != null)
                    unitsInTeam.Remove(unit);
            }
            if (teamIndex != -1)
            {
                _unitTeams.TryGetValue(teamIndex, out HashSet<Unit> newTeam);
                if (newTeam == null)
                    newTeam = new HashSet<Unit>();
                newTeam.Add(unit);
                _unitTeams[teamIndex] = newTeam;
            }

            unit.SetTeamIndex(teamIndex);

            OnUnitTeamChanged?.Invoke();
        }

        private void Unit_OnUnitDied(Unit unit, bool shouldDestroy = false)
        {
            unit.OnUnitDied -= Unit_OnUnitDied;

            //TODO: fix this when we fix the SetUnitTeamIndex stuff
            int unitTeam = unit.TeamIndex;
            RemoveUnitFromCombat(unit, shouldDestroy);

            int aliveTeams = 0;
            int winIndex = -1;
            foreach (var unitTeamPair in _unitTeams)
            {
                if (unitTeamPair.Value.Count > 0)
                {
                    aliveTeams++;
                    winIndex = unitTeamPair.Key;
                }
            }
            if (aliveTeams <= 1)
            {
                FinishCombat(winIndex);
            }
        }

        /// <summary>
        /// Remove unit from combat with optional choice to destroy gameobject. If not destroyed, unit remains at position but is taken off the grid and out of the combat units list.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="shouldDestroy"></param>
        public void RemoveUnitFromCombat(Unit unit, bool shouldDestroy = false, float delayBeforeDestroy = 2f)
        {
            _unitsInCombat.Remove(unit);
            _orderedUnits.Remove(unit);

            _tacticsGrid.RemoveUnitFromTile(unit.UnitGridIndex);

            SetUnitTeamIndex(unit, -1);

            if (shouldDestroy)
            {
                Destroy(unit.gameObject, delayBeforeDestroy);
            }
        }

        public bool TryActivateAbility(Ability ability, GridIndex origin, GridIndex target)
        {
            AbilityActivationData activationData;
            activationData.tacticsGrid = _tacticsGrid;
            activationData.originIndex = origin;
            activationData.targetIndex = target;

            ability.OnAbilityEnded += Ability_OnAbilityEnded;

            if (!ability.TryActivateAbility(activationData))
            {
                ability.OnAbilityEnded -= Ability_OnAbilityEnded;
                return false;
            }
            return true;
        }

        private void Ability_OnAbilityEnded(Ability ability)
        {
            ability.OnAbilityEnded -= Ability_OnAbilityEnded;

            if (_isCombatFinishing) return;

            OnAbilityUseCompleted?.Invoke();
            if (ability.GetAbilityOwner().CurrentActionPoints <= 0)
            {
                EndUnitTurn();
            }
        }

        public void ApplyEffectsToTarget(AbilitySystem instigator, AbilitySystem receiver, List<AbilityEffect> effectsToApply)
        {
            if (receiver == null)
                return;

            bool didHit = UnityEngine.Random.Range(0f, 1f) <= 0.85f;

            List<AbilityEffectReal> effectsRealList = new List<AbilityEffectReal>();
            for (int i = 0; i < effectsToApply.Count; i++)
            {
                AbilityEffectReal effectReal;
                effectReal.attributeType = effectsToApply[i].attributeType;
                effectReal.modifier = didHit ? StaticUtilities.MinMaxRandom(effectsToApply[i].minMaxModifier) : 0;
                effectsRealList.Add(effectReal);
            }
            receiver.ApplyEffects(effectsRealList);
        }

        public bool IsValidTileForUnit(Unit unit, GridIndex index)
        {
            if (!_tacticsGrid.IsIndexValid(index))
                return false;

            List<TileType> tileTypes = unit.UnitData.unitStats.validTileTypes;
            return tileTypes != null && tileTypes.Contains(_tacticsGrid.GridTiles[index].tileType);
        }

        public List<GridIndex> RemoveIndexesWithoutLineOfSight(GridIndex origin, List<GridIndex> tiles, float height, float offsetDistance)
        {
            List<GridIndex> returnList = new List<GridIndex>();
            for (int i = 0; i < tiles.Count; i++)
            {
                if (HasLineOfSight(origin, tiles[i], height, offsetDistance))
                {
                    returnList.Add(tiles[i]);
                }
            }
            return returnList;
        }

        public bool HasLineOfSight(GridIndex origin, GridIndex target, float height, float offsetDistance)
        {
            if (!_tacticsGrid.GetTileDataFromIndex(origin, out TileData originData))
            {
                return false;
            }
            if (!_tacticsGrid.GetTileDataFromIndex(target, out TileData targetData))
            {
                return false;
            }


            Vector3 startPosition = originData.tileMatrix.GetPosition();
            startPosition.y += height;

            Vector3 targetPosition = targetData.tileMatrix.GetPosition();
            targetPosition.y += height;

            Vector3 direction = targetPosition - startPosition;

            if (Physics.Raycast(startPosition, direction, out RaycastHit hitInfo, direction.magnitude))
            {

                Unit abilityUnit = originData.unitOnTile;
                Unit targetUnit = targetData.unitOnTile;
                Unit hitUnit = hitInfo.collider.GetComponent<Unit>();
                if (hitUnit != null)
                {
                    if (hitUnit != abilityUnit && hitUnit != targetUnit)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (offsetDistance > 0)
                    {
                        //Offset distances are scaled from 0 to 1, reflecting percentage from center to edge. We math that here.
                        float relativeDistance = (_tacticsGrid.TileSize.x / 2) * offsetDistance;
                        Vector2[] offsets = new Vector2[]
                        {
                        new Vector2(-relativeDistance, 0f),
                        new Vector2(0f, relativeDistance),
                        new Vector2(relativeDistance, 0f),
                        new Vector2(0f, -relativeDistance)
                        };
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            Vector3 startOffset = startPosition + new Vector3(offsets[i].x, 0f, offsets[i].y);

                            int unitLayer = 0;
                            if (abilityUnit)
                            {
                                unitLayer = abilityUnit.gameObject.layer;
                                abilityUnit.gameObject.layer = LayerMask.GetMask("Ignore Raycast");
                            }

                            if (!Physics.Raycast(startOffset, direction, out hitInfo, direction.magnitude))
                            {
                                if (abilityUnit)
                                    abilityUnit.gameObject.layer = unitLayer;
                                return true;
                            }
                            else
                            {
                                if (hitUnit = hitInfo.collider.GetComponent<Unit>())
                                {
                                    if (abilityUnit)
                                        abilityUnit.gameObject.layer = unitLayer;

                                    if (hitUnit != targetUnit)
                                        return false;
                                    else
                                        return true;
                                }
                            }
                            if (abilityUnit)
                                abilityUnit.gameObject.layer = unitLayer;
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        private void TacticsGrid_OnGridGenerated()
        {
            List<Unit> copyList = new List<Unit>(_unitsInCombat);
            for (int i = 0; i < copyList.Count; i++)
            {
                Unit unit = copyList[i];
                //GridIndex unitIndex = unit.UnitGridIndex;
                GridIndex positionIndex = _tacticsGrid.GetTileIndexFromWorldPosition(unit.transform.position);
                if (IsValidTileForUnit(unit, positionIndex))
                {
                    _tacticsGrid.AddUnitToTile(positionIndex, unit);
                }
                else
                {
                    RemoveUnitFromCombat(copyList[i], false);
                }
            }
        }

        private void TacticsGrid_OnTileDataUpdated(GridIndex index)
        {
            Unit unit = _unitsInCombat.FirstOrDefault(u => u.UnitGridIndex == index);
            if (unit)
            {
                if (IsValidTileForUnit(unit, index))
                {
                    return;
                }
                else
                {
                    RemoveUnitFromCombat(unit, false);
                }
            }
        }

        private void TacticsGrid_OnTileHeightChanged(GridIndex index)
        {
            Unit unit = _unitsInCombat.FirstOrDefault(u => u.UnitGridIndex == index);
            if (unit)
            {
                _tacticsGrid.GetTileDataFromIndex(index, out TileData tileData);
                unit.transform.position = new Vector3(unit.transform.position.x, tileData.tileMatrix.GetPosition().y, unit.transform.position.z);
            }
        }

        public List<GridIndex> RemoveNotWalkableIndexes(List<GridIndex> targetIndexes)
        {
            List<GridIndex> validIndexes = new List<GridIndex>();
            for (int i = 0; i < targetIndexes.Count; i++)
            {
                if (_tacticsGrid.IsTileWalkable(targetIndexes[i]))
                    validIndexes.Add(targetIndexes[i]);
            }
            return validIndexes;
        }

        public List<GridIndex> GetAbilityRange(GridIndex originIndex, AbilityRangeData rangeData)
        {
            List<GridIndex> indexesInRange = RemoveNotWalkableIndexes(AbilityStatics.GetIndexesFromPatternAndRange(originIndex, _tacticsGrid.GridShape, rangeData.rangeMinMax, rangeData.rangePattern));

            if (rangeData.lineOfSightData.requireLineOfSight)
            {
                indexesInRange = RemoveIndexesWithoutLineOfSight(originIndex, indexesInRange, rangeData.lineOfSightData.height, rangeData.lineOfSightData.offsetDistance);
            }
            return indexesInRange;
        }
    }
}