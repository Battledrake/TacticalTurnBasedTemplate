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
        private int _actionValue = 0;

        //HACK: SetTileType uses combobox but generic functionality is desired. Also two action buttons that Select Tile. This makes that work.
        private bool _isActive;

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
                OnSetActionValue(_actionValue);
                _isActive = true;
            }
        }

        //HACK: Used for ShowTileNeighborsAction to set an int using a toggle only that button has.
        public void OnSetActionValue(bool newValue)
        {
            OnSetActionValue(newValue ? 1 : 0);
        }

        //HACK: Used for SetTileType to set an int from a combo box only that button has.
        public void OnSetActionValue(int newValue)
        {
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
