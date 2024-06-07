using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class HealthVisual : MonoBehaviour
    {
        [SerializeField] private Transform _healthBar;
        [SerializeField] private GameObject _healthUnitPrefab;
        [SerializeField] private GameObject _floatingNumberPrefab;

        [SerializeField] private Color _damageColor;
        [SerializeField] private Color _healColor;
        [SerializeField] private float _healthChangeDelay = 0.5f;
        [SerializeField] private bool _isImmortal = false;

        public Transform HealthBar { get => _healthBar; }

        private IHealthVisual _owner;
        private List<GameObject> _healthUnits = new List<GameObject>();
        private Dictionary<int, SpriteRenderer> _healthUnitChildren = new Dictionary<int, SpriteRenderer>();
        private int _currentHealth = 0;
        private int _displayedHealth = 0;
        private int _maxHealth = 0;
        private Color _healthUnitColor = Color.red;

        public void InitHealthVisual(IHealthVisual owner)
        {
            _owner = owner;

            //TODO: Redo this to a pooling system like timeline visuals.
            for (int i = 0; i < _healthUnits.Count; i++)
            {
                Destroy(_healthUnits[i]);
            }
            _healthUnits.Clear();

            _maxHealth = owner.GetMaxHealth();
            _currentHealth = owner.GetHealth();
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

        public void UpdateHealthVisual(int amount)
        {
            if (!_isImmortal)
            {
                _currentHealth = _owner.GetHealth();

                StopCoroutine(UpdateHealthBar());
                StartCoroutine(UpdateHealthBar());
            }

            GameObject floatingNumber = Instantiate(_floatingNumberPrefab, _healthBar.position + _healthBar.forward, Quaternion.identity);
            Animator numberAnim = floatingNumber.GetComponentInChildren<Animator>();
            numberAnim.SetInteger("Random", UnityEngine.Random.Range(0, 2));
            numberAnim.speed = UnityEngine.Random.Range(0.8f, 1.2f);

            TextMeshPro textComp = floatingNumber.GetComponentInChildren<TextMeshPro>();
            if (amount == 0)
            {
                textComp.color = Color.yellow;
                textComp.text = "No Effect";
            }
            else
            {
                textComp.color = amount < 0 ? _damageColor : _healColor;
                textComp.text = amount.ToString();
            }

            Destroy(floatingNumber, 2f);
        }

        private void Update()
        {
            _healthBar.LookAt(Camera.main.transform);
        }

        private IEnumerator UpdateHealthBar()
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
                _displayedHealth = Mathf.Clamp(targetHealth, 0, _owner.GetMaxHealth());
            }
        }
    }
}