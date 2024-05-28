using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class TurnOrderBarController : MonoBehaviour
    {
        [SerializeField] private TurnOrderUnitDisplay _unitDisplayPrefab;
        [SerializeField] private Transform _displayContainer;
        [SerializeField] private int _displayPooledCount = 10;

        private List<TurnOrderUnitDisplay> _unitDisplays = new List<TurnOrderUnitDisplay>();

        private int _activeIndex;

        private void Awake()
        {
            for (int i = 0; i < _displayPooledCount; i++)
            {
                SpawnUnitDisplay();
            }

        }

        private void Start()
        {
            CombatManager.Instance.OnUnitTeamChanged += CombatManager_OnUnitTeamChanged;
            CombatManager.Instance.OnCombatStarted += CombatManager_OnCombatStarted;
            CombatManager.Instance.OnCombatEnded += CombatManager_OnCombatEnded;
            CombatManager.Instance.OnUnitTurnEnded += CombatManager_OnUnitTurnEnded;
            UpdateUnitsTurnBar();
        }

        private void CombatManager_OnCombatStarted()
        {
            _displayContainer.gameObject.SetActive(true);
            _activeIndex = 0;
        }

        private void CombatManager_OnCombatEnded()
        {
            _displayContainer.gameObject.SetActive(false);
            _activeIndex = -1;
        }

        private void CombatManager_OnUnitTurnEnded(Unit unit)
        {
        }

        private void CombatManager_OnUnitTeamChanged()
        {
            UpdateUnitsTurnBar();
        }

        private void UpdateUnitsTurnBar()
        {
            List<Unit> unitsInCombat = CombatManager.Instance.UnitsInCombat;
            if (unitsInCombat.Count > _unitDisplays.Count)
            {
                int addCount = unitsInCombat.Count - _unitDisplays.Count;
                for (int i = 0; i < addCount; i++)
                    SpawnUnitDisplay();
            }

            //for (int i = 0; i < _unitDisplays.Count; i++)
            //{
            //    Destroy(_unitDisplays[i]);
            //}
            //_unitDisplays.Clear();

            for(int i = 0; i < _unitDisplays.Count; i++)
            {
                _unitDisplays[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < unitsInCombat.Count; i++)
            {
                _unitDisplays[i].UpdateIcon(unitsInCombat[i]);
                _unitDisplays[i].gameObject.SetActive(true);
            }
        }

        private void SpawnUnitDisplay()
        {
            TurnOrderUnitDisplay unitDisplay = Instantiate(_unitDisplayPrefab, _displayContainer);
            _unitDisplays.Add(unitDisplay);
            unitDisplay.gameObject.SetActive(false);
        }
    }
}