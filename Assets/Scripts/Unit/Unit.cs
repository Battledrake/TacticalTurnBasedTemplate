using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    //Temporary for building out system.
    public enum UnitType
    {
        Warrior,
        Ranger,
        Slime
    }

    public class Unit : MonoBehaviour
    {
        [SerializeField] private UnitType _unitType = UnitType.Ranger;
        private GameObject _unitVisual;

        private Animator _unitAnimator;
        private UnitData _unitData;

        private void Awake()
        {
            LoadUnitData();
        }

        [ContextMenu("Load Unit Data")]
        public void LoadUnitData()
        {
            _unitData = DataManager.GetUnitDataFromType(_unitType);
            if (_unitVisual != null)
                Destroy(_unitVisual);
            _unitVisual = Instantiate(_unitData.unitVisual, this.transform);

            _unitAnimator = _unitVisual.GetComponent<Animator>();
        }
    }
}
