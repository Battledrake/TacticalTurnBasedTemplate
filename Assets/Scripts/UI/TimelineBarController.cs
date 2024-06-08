using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TimelineBarController : MonoBehaviour
    {
        [SerializeField] private TimelineUnitDisplay _unitDisplayPrefab;
        [SerializeField] private Transform _displayContainer;
        [SerializeField] private int _displayPooledCount = 10;

        private List<TimelineUnitDisplay> _pooledDisplays = new List<TimelineUnitDisplay>();
        private Dictionary<Unit, TimelineUnitDisplay> _unitDisplays = new Dictionary<Unit, TimelineUnitDisplay>();

        private void Awake()
        {
            for (int i = 0; i < _displayPooledCount; i++)
            {
                SpawnUnitDisplay();
            }
        }

        private void Start()
        {
            CombatManager.Instance.OnCombatStarted += CombatManager_OnCombatStarted;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
            CombatManager.Instance.OnActiveUnitChanged += CombatManager_OnActiveUnitChanged;
            CombatManager.Instance.OnActiveTeamChanged += PopulateTimelineBar;
            CombatManager.Instance.OnUnitAddedDuringCombat += CombatManager_OnUnitAddedDuringCombat;
            Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
        }

        private void OnDisable()
        {
            CombatManager.Instance.OnCombatStarted -= CombatManager_OnCombatStarted;
            CombatManager.Instance.OnCombatEnded -= CombatManager_OnCombatEnded;
            CombatManager.Instance.OnActiveUnitChanged -= CombatManager_OnActiveUnitChanged;
            CombatManager.Instance.OnActiveTeamChanged -= PopulateTimelineBar;
            CombatManager.Instance.OnUnitAddedDuringCombat -= CombatManager_OnUnitAddedDuringCombat;
            Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
        }

        private void Unit_OnAnyUnitDied(Unit unit)
        {
            if (_unitDisplays.ContainsKey(unit))
            {
                _unitDisplays[unit].gameObject.SetActive(false);
                _unitDisplays.Remove(unit);
            }
        }

        private void CombatManager_OnCombatStarted()
        {
            _displayContainer.gameObject.SetActive(true);
            PopulateTimelineBar();
        }

        private void CombatManager_OnCombatEnded()
        {
            _displayContainer.gameObject.SetActive(false);
        }

        private void CombatManager_OnActiveUnitChanged(Unit unit)
        {
            if(CombatManager.Instance.TurnOrderType != TurnOrderType.Team)
            {
                if(_unitDisplays.TryGetValue(unit, out TimelineUnitDisplay unitDisplay))
                {
                    _displayContainer.GetChild(0).SetAsLastSibling();
                    unitDisplay.transform.SetAsFirstSibling();
                }
            }
        }

        private void PopulateTimelineBar()
        {
            List<Unit> orderedUnits = CombatManager.Instance.OrderedUnits;
            _unitDisplays.Clear();

            if (orderedUnits.Count > _pooledDisplays.Count)
            {
                int addCount = orderedUnits.Count - _pooledDisplays.Count;
                for (int i = 0; i < addCount; i++)
                    SpawnUnitDisplay();
            }

            for(int i = 0; i < _pooledDisplays.Count; i++)
            {
                _pooledDisplays[i].gameObject.SetActive(false);
            }

            _pooledDisplays.OrderBy(i => i.transform.GetSiblingIndex());


            for (int i = 0; i < orderedUnits.Count; i++)
            {
                _unitDisplays.TryAdd(orderedUnits[i], _pooledDisplays[i]);
            }

            foreach(var unitDisplayPair in _unitDisplays)
            {
                unitDisplayPair.Value.UpdateIcon(unitDisplayPair.Key);
                unitDisplayPair.Value.gameObject.SetActive(true);
            }
        }

        private void CombatManager_OnUnitAddedDuringCombat(Unit unit)
        {
            if(_pooledDisplays.Count < _unitDisplays.Count + 1)
            {
                SpawnUnitDisplay();
            }

            _unitDisplays.TryAdd(unit, _pooledDisplays.Find(c => !c.gameObject.activeInHierarchy));
            _unitDisplays[unit].UpdateIcon(unit);
            _unitDisplays[unit].gameObject.SetActive(true);

            if (CombatManager.Instance.TurnOrderType != TurnOrderType.Team)
            {
                _unitDisplays[unit].transform.SetSiblingIndex(1);
            }
        }

        private void SpawnUnitDisplay()
        {
            TimelineUnitDisplay unitDisplay = Instantiate(_unitDisplayPrefab, _displayContainer);
            _pooledDisplays.Add(unitDisplay);
            unitDisplay.gameObject.SetActive(false);
        }
    }
}