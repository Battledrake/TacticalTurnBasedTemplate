using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class WidgetSwitcher : MonoBehaviour
    {
        [SerializeField] private int _activeIndex = -1;
        private List<GameObject> _managedWidgets;

        private void OnValidate()
        {
            _managedWidgets = new List<GameObject>();

            for (int i = 0; i < this.transform.childCount; i++)
            {
                GameObject childObject = this.transform.GetChild(i).gameObject;
                _managedWidgets.Add(childObject);
                childObject.SetActive(false);
            }

            if (_activeIndex > -1 && _activeIndex < _managedWidgets.Count)
                _managedWidgets[_activeIndex].SetActive(true);
        }

        private void Start()
        {
            for(int i = 0; i < _managedWidgets.Count; i++)
            {
                _managedWidgets[i].SetActive(true);
                _managedWidgets[i].SetActive(false);
            }
        }

        public void SetActiveWidget(int widgetIndex)
        {
            if (widgetIndex == _activeIndex)
            {
                _managedWidgets[_activeIndex].SetActive(false);
                _activeIndex = -1;
            }
            else if (_managedWidgets.Count > widgetIndex)
            {
                if (_activeIndex > -1)
                    _managedWidgets[_activeIndex].SetActive(false);

                _managedWidgets[widgetIndex].SetActive(true);
                _activeIndex = widgetIndex;
            }
        }

    }
}
