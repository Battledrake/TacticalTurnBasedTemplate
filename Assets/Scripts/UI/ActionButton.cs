using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [RequireComponent(typeof(Toggle))]
    public class ActionButton : MonoBehaviour
    {
        [SerializeField] private string _actionName;
        [SerializeField] private ActionBase _leftClickAction;
        [SerializeField] private ActionBase _rightClickAction;
        [SerializeField] private PlayerActions _playerActions;
        [SerializeField] private TextMeshProUGUI _buttonLabel;

        private Toggle _buttonToggle;
        private int _actionValue = -1;

        //HACK: SetTileType uses combobox but generic functionality is desired. Also two action buttons that Select Tile. This makes that work.
        private bool _isActive = false;

        private void Awake()
        {
            _buttonToggle = this.GetComponent<Toggle>();
            _buttonToggle.onValueChanged.AddListener(OnButtonClicked);
        }

        private void OnValidate()
        {
            _buttonLabel.text = _actionName;
        }

        private void OnSelectedActionsChanged(ActionBase leftAction, ActionBase rightAction)
        {
            if (_isActive)
            {
                _isActive = false;
                _buttonToggle.isOn = false;
            }
            else
            {
                _isActive = true;
            }
        }

        //HACK: Used for ShowTileNeighborsAction to set an int using a toggle only that button has.
        public void SetActionValue(bool newValue)
        {
            SetActionValues(newValue ? 1 : 0);
        }

        //HACK: Used for SetTileType and UnitSelection actions. Default values set on action prefabs.
        public void SetActionValues(int newValue)
        {
            if (_actionValue == newValue)
                return;

            _actionValue = newValue;

            if (_leftClickAction != null)
                _playerActions.LeftClickAction.actionValue = _actionValue;
            if (_rightClickAction != null)
                _playerActions.RightClickAction.actionValue = 0;
        }

        public void OnButtonClicked(bool isDown)
        {
            if (isDown)
            {
                _playerActions.OnSelectedActionsChanged += OnSelectedActionsChanged;
                _playerActions.SetSelectedActions(_leftClickAction, _rightClickAction);
            }
            else
            {
                if (_isActive)
                {
                    _isActive = false;
                    _playerActions.ClearSelectedActions();
                }
                _playerActions.OnSelectedActionsChanged -= OnSelectedActionsChanged;
            }
        }
    }
}
