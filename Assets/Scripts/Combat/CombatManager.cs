using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
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

        //Do we need all these events? There's a lot happening, and they all have uses. Maybe a TurnManager class to separate some of this out.
        public event Action<Unit, GridIndex> OnUnitGridIndexChanged;
        public event Action OnUnitTeamChanged;
        public event Action OnCombatStarted;
        public event Action<int> OnCombatFinishing;
        public event Action OnCombatEnded;
        public event Action OnPlayerTurnStarted;
        public event Action OnPlayerTurnEnded;
        public event Action OnActiveTeamChanged;
        public event Action<Unit> OnActiveUnitChanged;
        public event Action<Unit> OnUnitAddedDuringCombat;
        public event Action OnActionStarted;
        public event Action OnActionEnded;

        [SerializeField] TurnOrderType _turnOrderType;
        [SerializeField] private List<TeamColorData> _teamColors;
        [SerializeField] private UnitAI _unitAIPrefab;
        [SerializeField] private float _endTurnDelay = 1f;

        [SerializeField] private GameplayEffect _halfCoverEffect;
        [SerializeField] private GameplayEffect _fullCoverEffect;

        [Header("Dependencies")]
        [SerializeField] private TacticsGrid _tacticsGrid;

        public TurnOrderType TurnOrderType { get => _turnOrderType; set => _turnOrderType = value; }
        public bool ShowEnemyMoveRange { get => _showEnemyMoveRange; set => _showEnemyMoveRange = value; }
        public List<Unit> UnitsInCombat => _unitsInCombat;
        public Dictionary<int, HashSet<Unit>> UnitTeams => _unitTeams;
        public List<Unit> OrderedUnits => _orderedUnits;
        public int NumberOfTeams => _teamColors.Count;
        public Unit ActiveUnit => _activeUnit;
        public bool IsInCombat => _isInCombat;
        public bool IsCombatFinishing => _isCombatFinishing;

        private List<Unit> _unitsInCombat = new List<Unit>();
        private Dictionary<int, HashSet<Unit>> _unitTeams = new Dictionary<int, HashSet<Unit>>();
        private List<Unit> _orderedUnits = new List<Unit>();

        private bool _isInCombat = false;
        private bool _isCombatFinishing = false;
        private bool _showEnemyMoveRange = false;

        private Unit _activeUnit = null;
        private int _activeTeamIndex = -1;
        private bool _isAIControlledTeam = false;
        private float _lastEndTurnTime = 0f;

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

        public void SetUnitUseAI(Unit unit, bool isUsingAI)
        {
            if (isUsingAI)
            {
                unit.SetUnitAI(_unitAIPrefab);
            }
            else
            {
                unit.SetUnitAI(null);
            }
        }

        private void OrderUnitsByTeam()
        {
            if (_unitTeams.TryGetValue(_activeTeamIndex, out HashSet<Unit> teamUnits))
            {
                if (teamUnits != null && teamUnits.Count > 0)
                {
                    foreach (Unit unit in teamUnits)
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

            if (_turnOrderType == TurnOrderType.Team)
            {
                SetActiveTeamIndex();
            }

            OrderUnitsByTurnOrderType();

            for (int i = 0; i < _unitsInCombat.Count; i++)
            {
                _unitsInCombat[i].CombatStarted();
            }

            _isInCombat = true;
            _isCombatFinishing = false;
            OnCombatStarted?.Invoke();

            _activeUnit = null;


            _isAIControlledTeam = _orderedUnits[0].UnitAI != null;

            StartTurn();
        }

        private void StartTurn()
        {
            //NOTE: Ensure turn start is called before setting the active unit. This allows the turn start logic, like updating ability system stuff, to happen before OnActiveUnitChanged is fired.
            //ActiveUnitChanged enables various UI displays that depend on AbilitySystem values to be updated like ActionPoints and Cooldowns.
            if (_turnOrderType == TurnOrderType.Team)
            {
                if (_isAIControlledTeam)
                {
                    SetNextTeamUnitAsActive();
                    _activeUnit.TurnStarted();
                }
                else
                {
                    OnPlayerTurnStarted?.Invoke();
                    for (int i = 0; i < _orderedUnits.Count; i++)
                    {
                        _orderedUnits[i].TurnStarted();
                    }
                    SetNextTeamUnitAsActive();
                }
            }
            else
            {
                _activeUnit = GetNextOrderedUnit();

                if (_activeUnit.UnitAI != null)
                {
                    OnPlayerTurnEnded?.Invoke();
                }
                else
                {
                    OnPlayerTurnStarted?.Invoke();
                }

                _activeUnit.TurnStarted();

                OnActiveUnitChanged?.Invoke(_activeUnit);
            }

            //TODO: Make controller a singleton? Or different way of handling.
            GameObject.Find("[Cameras]").GetComponent<CameraController>().SetMoveToTarget(_activeUnit.transform.position);
        }

        private Unit GetNextOrderedUnit()
        {
            int activeIndex = _orderedUnits.IndexOf(_activeUnit);
            return _orderedUnits[++activeIndex % _orderedUnits.Count];
        }

        public void SetNextTeamUnitAsActive()
        {
            if (_turnOrderType != TurnOrderType.Team) return;

            Unit currentUnit = _activeUnit;
            if (_orderedUnits.Count > 0)
            {
                int activeIndex = _orderedUnits.IndexOf(_activeUnit);
                _activeUnit = _orderedUnits[++activeIndex % _orderedUnits.Count];

                if (currentUnit != _activeUnit)
                    OnActiveUnitChanged.Invoke(_activeUnit);
            }
        }

        private void NextTurn()
        {
            if (_turnOrderType == TurnOrderType.Team)
            {
                _orderedUnits.Clear();

                if (!_isAIControlledTeam)
                    OnPlayerTurnEnded?.Invoke();

                SetActiveTeamIndex();
                OrderUnitsByTeam();

                _isAIControlledTeam = _orderedUnits[0].UnitAI != null;
                OnActiveTeamChanged?.Invoke();
            }
            //Temporary as starting next turns too fast feels bad.
            StartCoroutine(WaitToStartTurn());
            IEnumerator WaitToStartTurn()
            {
                yield return new WaitForSeconds(1f);
                StartTurn();
            }
        }

        //For AI to cancel their turn if logic allows. AI might put in request after its turn was ended by CombatManager and is no longer the active unit so we stop it here.
        public void RequestEndTurn(Unit unit)
        {
            if (_isCombatFinishing) return;

            if (unit == _activeUnit)
            {
                if (Time.time > _lastEndTurnTime + _endTurnDelay)
                {
                    _lastEndTurnTime = Time.time;
                    EndUnitTurn();
                }
                else
                {
                    Debug.LogWarning("End Turn called before delay finished. Ignoring.");
                }
            }
        }

        //Player can cancel their turn manually with End Turn button. We just ensure they're not somehow doing so when it's the AI's turn.
        public void PlayerRequestEndTurn()
        {
            if (_activeUnit.UnitAI == null)
                RequestEndTurn(_activeUnit);
        }

        private void EndUnitTurn()
        {
            if (_turnOrderType == TurnOrderType.Team)
            {
                _activeUnit.TurnEnded(); //Used for timeline displays and AI cancelling.
                _orderedUnits.Remove(_activeUnit);
                _activeUnit = null;

                if (_orderedUnits.Count <= 0)
                {
                    NextTurn();
                }
                else
                {
                    SetNextTeamUnitAsActive();

                    if (_isAIControlledTeam)
                        _activeUnit.TurnStarted();
                }
            }
            else
            {
                NextTurn();
            }
        }

        private void SetActiveTeamIndex()
        {
            //First we check if there are any valid teams with an index greater than the current index.
            var teamIndexes = _unitTeams.Keys.Where(k => k > _activeTeamIndex && _unitTeams[k]?.Count > 0).OrderBy(k => k);
            if (teamIndexes.Any())
            {
                _activeTeamIndex = teamIndexes.First();
            }
            else
            {
                //No greater indexes found. We return the lowest valid index.
                _activeTeamIndex = _unitTeams.Keys.Min();
            }
        }

        //This allows team unit selection from the Timeline UI bar or. (Only available during TurnOrderType.Team)
        public void SetActiveTeamUnit(Unit unit)
        {
            if (_activeUnit == unit) return;

            if (_orderedUnits.Contains(unit))
            {
                _activeUnit = unit;
                OnActiveUnitChanged?.Invoke(_activeUnit);
            }
        }

        //Called specifically by the end combat button on the finished combat panel
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

        //Called by Game conditions or End Combat button.
        public void FinishCombat(int winTeamIndex)
        {
            StopAllCoroutines();
            _isCombatFinishing = true;
            OnCombatFinishing?.Invoke(winTeamIndex);
        }

        public void MoveUnit(Unit unit, List<GridIndex> path, float pathLength)
        {
            GridMovement gridMoveComp = unit.GridMovement;
            if (gridMoveComp)
            {
                unit.OnUnitReachedDestination += Unit_OnUnitReachedDestination;
                gridMoveComp.SetPathAndMove(path);

                OnActionStarted?.Invoke();
            }
            else
            {
                Debug.LogWarning("Unit does not have a grid movement component. Cancelling Operation");
                unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;
                return;
            }

            int moveCost = pathLength <= unit.MoveRange ? -1 : -2;

            GameplayEffect costEffect = new GameplayEffect();
            costEffect.durationData.durationPolicy = EffectDurationPolicy.Instant;
            costEffect.attribute = AttributeId.ActionPoints;
            costEffect.magnitude = moveCost;

            unit.AbilitySystem.ApplyEffect(costEffect);

            _tacticsGrid.RemoveUnitFromTile(unit.GridIndex);
            _tacticsGrid.AddUnitToTile(path.Last(), unit, false);
            OnUnitGridIndexChanged?.Invoke(unit, path.Last());
        }

        public void TeleportUnit(Unit unit, GridIndex targetIndex)
        {
            _tacticsGrid.RemoveUnitFromTile(unit.GridIndex);
            _tacticsGrid.AddUnitToTile(targetIndex, unit, true, true);
        }

        private void Unit_OnUnitReachedDestination(Unit unit)
        {
            unit.OnUnitReachedDestination -= Unit_OnUnitReachedDestination;

            OnActionEnded?.Invoke();

            if (unit.AbilitySystem.GetAttributeCurrentValue(AttributeId.ActionPoints) <= 0)
                RequestEndTurn(unit);
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
                    {
                        _orderedUnits.Add(unit);
                        unit.TurnStarted();
                    }
                    else
                    {
                        return;
                    }

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

            if (_activeUnit == unit)
                RequestEndTurn(unit);

            int unitTeam = unit.TeamIndex;
            RemoveUnitFromCombat(unit, shouldDestroy);

            if (!_isInCombat) return;

            int aliveTeams = 0;
            int winIndex = -1;
            foreach (KeyValuePair<int, HashSet<Unit>> unitTeamPair in _unitTeams)
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

            _tacticsGrid.RemoveUnitFromTile(unit.GridIndex);

            SetUnitTeamIndex(unit, -1);

            if (shouldDestroy)
            {
                Destroy(unit.gameObject, delayBeforeDestroy);
            }
        }

        public bool TryUseAbility(Ability ability, GridIndex origin, GridIndex target)
        {
            AbilityActivationData activationData;
            activationData.tacticsGrid = _tacticsGrid;
            activationData.originIndex = origin;
            activationData.targetIndex = target;

            if (ability.CanActivateAbility(activationData))
            {
                ability.OnAbilityEnded += Ability_OnAbilityEnded;
                OnActionStarted?.Invoke();

                ability.ActivateAbility(activationData);
                return true;
            }
            return false;
        }

        private void Ability_OnAbilityEnded(Ability ability)
        {
            ability.OnAbilityEnded -= Ability_OnAbilityEnded;
            OnActionEnded?.Invoke();

            if (ability.EndTurnOnUse || ability.AbilityOwner.GetAttributeCurrentValue(AttributeId.ActionPoints) <= 0)
            {
                RequestEndTurn(ability.AbilityOwner.OwningUnit);
            }
        }

        public void ApplyAbilityEffectsToTarget(AbilitySystem instigator, AbilitySystem receiver, Ability ability)
        {
            if (receiver == null)
                return;

            bool didHit = true;

            if (!ability.IgnoresCover)
            {
                GameplayEffect coverEffect = CalculateCoverEffect(instigator, receiver);
                if (coverEffect.magnitude != 0)
                {
                    int instigatorAim = instigator.GetAttributeCurrentValue(AttributeId.Aim);
                    int receiverDefense = instigator.GetAttributeCurrentValue(AttributeId.Defense) + coverEffect.magnitude;

                    float random = UnityEngine.Random.Range(0f, 100f);
                    int hitChance = instigatorAim - receiverDefense;
                    didHit = random <= hitChance;
                    Debug.Log($"HitChance: {hitChance}, Rolled: {random}");
                }
            }

            if (!didHit)
            {
                Debug.Log("Missed!");
                return;
            }


            List<GameplayEffect> effectsRealList = ConvertRangedEffectsToSingleMagnitudes(ability.Effects);

            for (int i = 0; i < effectsRealList.Count; i++)
            {
                receiver.ApplyEffect(effectsRealList[i]);
            }
        }

        private List<GameplayEffect> ConvertRangedEffectsToSingleMagnitudes(List<RangedGameplayEffect> rangedEffects)
        {
            List<GameplayEffect> convertedEffects = new List<GameplayEffect>();
            for (int i = 0; i < rangedEffects.Count; i++)
            {
                GameplayEffect effectReal;
                effectReal.durationData = rangedEffects[i].durationData;
                effectReal.attribute = rangedEffects[i].attribute;
                effectReal.magnitude = StaticUtilities.MinMaxRandom(rangedEffects[i].magnitudeRange);
                convertedEffects.Add(effectReal);
            }
            return convertedEffects;
        }

        private GameplayEffect CalculateCoverEffect(AbilitySystem instigator, AbilitySystem receiver)
        {
            if (receiver.OwningUnit)
            {
                _tacticsGrid.GetTileDataFromIndex(receiver.OwningUnit.GridIndex, out TileData receiverTile);
                if (!receiverTile.cover.hasCover)
                    return default;

                if (!instigator.OwningUnit)
                    return default;

                _tacticsGrid.GetTileDataFromIndex(instigator.OwningUnit.GridIndex, out TileData instigatorTile);

                GridIndex direction = instigatorTile.index - receiverTile.index;
                float angle = Mathf.Atan2(direction.z, direction.x);
                float angleToDegrees = angle * 180 / Mathf.PI;

                GridIndex directionOfAttack = ConvertDegreesToDirection(angleToDegrees);

                for(int i = 0; i < receiverTile.cover.data.Count; i++)
                {
                    if (receiverTile.cover.data[i].direction == directionOfAttack)
                    {
                        switch (receiverTile.cover.data[i].coverType)
                        {
                            case CoverType.HalfCover:
                                return _halfCoverEffect;
                            case CoverType.FullCover:
                                return _fullCoverEffect;
                        }
                    }
                }
            }
            return default;
        }

        public static GridIndex ConvertDegreesToDirection(float degrees)
        {
            if (degrees >= 45f && degrees <= 135f)
                return new GridIndex(0, 1);
            if (degrees >= 135f && degrees <= 180f || degrees >= -180f && degrees <= -135f)
                return new GridIndex(-1, 0);
            if (degrees <= 45f && degrees >= -45f)
                return new GridIndex(1, 0);
            if (degrees <= -45f && degrees >= -135f)
                return new GridIndex(0, -1);

            return GridIndex.Invalid();
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
                if (_tacticsGrid.GetTileDataFromIndex(origin, out TileData originData) && _tacticsGrid.GetTileDataFromIndex(tiles[i], out TileData targetData))
                {
                    if (AbilityStatics.HasLineOfSight(originData, targetData, height, offsetDistance))
                    {
                        returnList.Add(tiles[i]);
                    }
                }
            }
            return returnList;
        }

        private void TacticsGrid_OnGridGenerated()
        {
            List<Unit> copyList = new List<Unit>(_unitsInCombat);
            for (int i = 0; i < copyList.Count; i++)
            {
                Unit unit = copyList[i];
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
            Unit unit = _unitsInCombat.FirstOrDefault(u => u.GridIndex == index);
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
            Unit unit = _unitsInCombat.FirstOrDefault(u => u.GridIndex == index);
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