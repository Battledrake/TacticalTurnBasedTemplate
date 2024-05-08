using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TTBTk
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

        private void Awake()
        {
            _buttonToggle = this.GetComponent<Toggle>();
            _buttonToggle.onValueChanged.AddListener(OnButtonClicked);
        }

        private void OnValidate()
        {
            _buttonLabel.text = _actionName;
        }

        public void OnButtonClicked(bool isDown)
        {
            if (isDown)
            {
                _playerActions.SetSelectedActions(_leftClickAction, _rightClickAction);
            }
            else
            {
                _playerActions.ClearSelectedActions();
            }
        }
    }
}
