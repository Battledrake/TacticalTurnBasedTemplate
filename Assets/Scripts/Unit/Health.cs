using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class Health : MonoBehaviour
    {
        public event Action OnHealthReachedZero;

        [SerializeField] private Transform _healthBar;
        [SerializeField] private GameObject _healthUnitPrefab;
        [SerializeField] private GameObject _floatingNumberPrefab;

        [SerializeField] private Color _damageColor;
        [SerializeField] private Color _healColor;
        [SerializeField] private float _healthChangeDelay = 0.5f;
        [SerializeField] private bool _isImmortal = false;

        public Transform HealthBar { get => _healthBar; }

        private Unit _owner;
        private List<GameObject> _healthUnits = new List<GameObject>();
        private Dictionary<int, SpriteRenderer> _healthUnitChildren = new Dictionary<int, SpriteRenderer>();
        private int _currentHealth = 0;
        private int _displayedHealth = 0;
        private int _maxHealth = 0;
        private Color _healthUnitColor = Color.red;

        public void Start()
        {
            _owner = this.GetComponent<Unit>();

            _maxHealth = _owner.UnitData.unitStats.maxHealth;
            _currentHealth = _maxHealth;
            _displayedHealth = _currentHealth;
            for (int i = 0; i < _maxHealth; i++)
            {
                GameObject healthUnit = Instantiate(_healthUnitPrefab, _healthBar);
                _healthUnits.Add(healthUnit);
                SpriteRenderer healthUnitRenderer = healthUnit.transform.GetChild(0).GetComponent<SpriteRenderer>();
                healthUnitRenderer.color = _healthUnitColor;
                _healthUnitChildren.TryAdd(i + 1, healthUnitRenderer);
            }
        }

        public void SetHealthUnitColor(Color color)
        {
            _healthUnitColor = color;

            foreach (var healthUnit in _healthUnitChildren.Values)
            {
                healthUnit.color = _healthUnitColor;
            }
        }

        public void DisplayHealthBar(bool shouldDisplay)
        {
            _healthBar.gameObject.SetActive(shouldDisplay);
        }

        public void UpdateHealth(int amount)
        {
            if (!_isImmortal)
            {
                _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
                if (_currentHealth == 0)
                {
                    OnHealthReachedZero?.Invoke();
                }
                StopCoroutine(UpdateHealthVisual());
                StartCoroutine(UpdateHealthVisual());
            }

            GameObject floatingNumber = Instantiate(_floatingNumberPrefab, _healthBar.position + _healthBar.forward, Quaternion.identity);
            Animator numberAnim = floatingNumber.GetComponentInChildren<Animator>();
            numberAnim.SetInteger("Random", UnityEngine.Random.Range(0, 2));
            numberAnim.speed = UnityEngine.Random.Range(0.8f, 1.2f);

            TextMeshPro textComp = floatingNumber.GetComponentInChildren<TextMeshPro>();
            textComp.color = amount < 0 ? _damageColor : _healColor;
            textComp.text = amount.ToString();

            Destroy(floatingNumber, 2f);
        }

        private void Update()
        {
            _healthBar.LookAt(Camera.main.transform);
        }

        private IEnumerator UpdateHealthVisual()
        {

            while (_displayedHealth != _currentHealth)
            {
                int step = _displayedHealth > _currentHealth ? -1 : 1;
                int targetHealth = _currentHealth;

                yield return new WaitForSeconds(_healthChangeDelay);

                int indexStep = _displayedHealth > _currentHealth ? 0 : 1;

                for (int i = _displayedHealth + indexStep; i != targetHealth + indexStep; i += step)
                {
                    if (_healthUnitChildren.TryGetValue(i, out SpriteRenderer healthUnitChild))
                        healthUnitChild.enabled = step > 0;

                    yield return new WaitForSeconds(_healthChangeDelay);
                }
                _displayedHealth = Mathf.Clamp(targetHealth, 0, _maxHealth);
            }
        }
    }
}