using UnityEngine;
using UnityEngine.UI;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class DebugMenu : MonoBehaviour
    {
        [SerializeField] private Button[] _tabButtons;
        [SerializeField] private Color _tabBaseColor;
        [SerializeField] private Color _tabSelectedColor;

        private int _activeTabIndex = -1;

        public void OnTabClicked(int tabIndex)
        {
            if (tabIndex == _activeTabIndex)
            {
                _tabButtons[_activeTabIndex].GetComponent<Image>().color = _tabBaseColor;
                _activeTabIndex = -1;
            }
            else
            {
                if (_activeTabIndex > -1)
                    _tabButtons[_activeTabIndex].GetComponent<Image>().color = _tabBaseColor;

                _tabButtons[tabIndex].GetComponent<Image>().color = _tabSelectedColor;
                _activeTabIndex = tabIndex;
            }
        }
    }
}
