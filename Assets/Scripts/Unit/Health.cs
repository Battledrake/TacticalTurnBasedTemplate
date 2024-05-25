using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class Health : MonoBehaviour
    {
        public event Action OnHealthReachedZero;

        [SerializeField] private Transform _healthBar;

        [SerializeField] private GameObject _healthUnitPrefab;
        [SerializeField] private bool _updateHealth = false;

        [SerializeField] private float _healthChangeDelay = 0.5f;

        private Unit _owner;
        private List<GameObject> _healthUnits = new List<GameObject>();
        private Dictionary<int, SpriteRenderer> _healthUnitChildren = new Dictionary<int, SpriteRenderer>();
        private int _currentHealth = 0;
        private int _maxHealth = 0;

        public void Start()
        {
            _owner = this.GetComponent<Unit>();

            _maxHealth = _owner.UnitData.unitStats.maxHealth;
            _currentHealth = _maxHealth;
            for (int i = 0; i < _maxHealth; i++)
            {
                GameObject healthUnit = Instantiate(_healthUnitPrefab, _healthBar);
                _healthUnits.Add(healthUnit);
                _healthUnitChildren.TryAdd(i + 1, healthUnit.transform.GetChild(0).GetComponent<SpriteRenderer>());
            }
        }

        public void UpdateHealth(int amount)
        {
            _updateHealth = true;
            StartCoroutine(UpdateHealthVisual(amount));
        }

        [ContextMenu("AddHealth(2)")]
        public void ContextAddHealth()
        {
            _updateHealth = true;
            StartCoroutine(UpdateHealthVisual(2));
        }

        [ContextMenu("RemoveHealth(2)")]
        public void ContextRemoveHealth()
        {
            _updateHealth = true;
            StartCoroutine(UpdateHealthVisual(-2));
        }

        private void Update()
        {
            _healthBar.LookAt(Camera.main.transform);
        }

        private IEnumerator UpdateHealthVisual(int amountChanged)
        {
            while (_updateHealth)
            {
                int step = amountChanged < 0 ? -1 : 1;
                int targetHealth = _currentHealth + amountChanged;

                if (targetHealth <= 0)
                    OnHealthReachedZero?.Invoke();

                int indexStep = amountChanged < 0 ? 0 : 1;

                for (int i = _currentHealth + indexStep; i != targetHealth + indexStep; i += step)
                {
                    if (_healthUnitChildren.TryGetValue(i, out SpriteRenderer healthUnitChild))
                        healthUnitChild.enabled = step > 0;

                    yield return new WaitForSeconds(_healthChangeDelay);
                }
                _currentHealth = Mathf.Clamp(targetHealth, 0, _maxHealth);

                _updateHealth = false;
            }
        }
    }
}