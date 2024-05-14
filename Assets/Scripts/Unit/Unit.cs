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
        Slime,
        Orc,
        Mech6
    }

    public class Unit : MonoBehaviour, IUnitAnimation
    {
        [SerializeField] private UnitType _unitType = UnitType.Ranger;
        [SerializeField] private Color _hoverColor;
        [SerializeField] private Color _selectedColor = Color.green;

        public GridIndex UnitGridIndex { get => _gridIndex; set => _gridIndex = value; }

        private GameObject _unitVisual;
        private Animator _unitAnimator;
        private UnitData _unitData;
        private GridIndex _gridIndex = new GridIndex(int.MinValue, int.MinValue);
        private Outline _unitOutline;
        private float _defaultOutlineWidth = 2f;
        private float _hoverSelectedWidth = 3f;

        private bool _isHovered = false;
        private bool _isSelected = false;

        public void InitializeUnit(UnitType unitType)
        {
            if (_unitVisual != null)
                Destroy(_unitVisual);

            _unitType = unitType;
            _unitData = DataManager.GetUnitDataFromType(_unitType);
            if (_unitVisual != null)
                Destroy(_unitVisual);
            _unitVisual = Instantiate(_unitData.assetData.unitVisual, this.transform);

            _unitAnimator = _unitVisual.GetComponent<Animator>();
            _unitOutline = _unitVisual.GetComponent<Outline>();
        }

        [ContextMenu("ChangeType")]
        public void ChangeUnitType()
        {
            InitializeUnit(_unitType);
        }

        [ContextMenu("AddToCombat")]
        public void AddUnitToCombat()
        {
            GameObject.Find("[CombatSystem]").GetComponent<CombatSystem>().AddUnitToCombat(this.transform.position, this);
            _unitAnimator.SetTrigger("Respawn");
        }

        public void SetIsHovered(bool isHovered)
        {
            _isHovered = isHovered;
            UpdateOutlineVisual();
        }

        public void SetIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            UpdateOutlineVisual();
        }

        public void UpdateOutlineVisual()
        {
            if (!_unitOutline)
                return;

            if (!_isSelected && !_isHovered)
            {
                _unitOutline.enabled = false;
                return;
            }
            _unitOutline.enabled = true;

            if (_isSelected)
            {
                _unitOutline.OutlineColor = _selectedColor;

                if (_isHovered)
                    _unitOutline.OutlineWidth = _hoverSelectedWidth;
                else
                    _unitOutline.OutlineWidth = _defaultOutlineWidth;
            }
            else
            {
                _unitOutline.OutlineColor = _hoverColor;
                _unitOutline.OutlineWidth = _defaultOutlineWidth;
            }
        }

        public void PlayDeathAnimation()
        {
            _unitAnimator.SetTrigger("Die");
        }
    }
}
