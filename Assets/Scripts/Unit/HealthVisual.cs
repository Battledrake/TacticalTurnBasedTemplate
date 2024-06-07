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
        [SerializeField] private float _healthChangeDelay = 0.2f;

        public Transform HealthBar { get => _healthBar; }

        private IHealthVisual _owner;
        private Dictionary<int, SpriteRenderer> _healthUnitChildren = new Dictionary<int, SpriteRenderer>();
        private int _displayedHealth = 0;
        private Color _healthUnitColor = Color.red;

        public void InitHealthVisual(IHealthVisual owner)
        {
            _owner = owner;
        }

        private void SpawnHealthUnitVisual(int healthValue)
        {
            GameObject healthUnit = Instantiate(_healthUnitPrefab, _healthBar);
            SpriteRenderer healthUnitRenderer = healthUnit.transform.GetChild(0).GetComponent<SpriteRenderer>();
            healthUnitRenderer.color = _healthUnitColor;
            _healthUnitChildren.TryAdd(healthValue, healthUnitRenderer);
        }

        public void UpdateHealthVisual()
        {
            if (_healthUnitChildren.Count < _owner.GetMaxHealth())
            {
                int currentCount = _healthUnitChildren.Count;
                int healthDiff = _owner.GetMaxHealth() - currentCount;
                for (int i = 1; i <= healthDiff; i++)
                {
                    SpawnHealthUnitVisual(currentCount + i);
                }
            }

            foreach (var healthUnitPair in _healthUnitChildren)
            {
                healthUnitPair.Value.transform.parent.gameObject.SetActive(false);
            }

            for (int i = 1; i <= _owner.GetMaxHealth(); i++)
            {
                _healthUnitChildren[i].transform.parent.gameObject.SetActive(true);
                if (_owner.GetHealth() >= i)
                    _healthUnitChildren[i].enabled = true;
                else
                    _healthUnitChildren[i].enabled = false;
            }

            _displayedHealth = _owner.GetHealth();
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

        public void DisplayHealthChange(int amount)
        {
            StopCoroutine(UpdateHealthBar());
            StartCoroutine(UpdateHealthBar());

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

            while (_displayedHealth != _owner.GetHealth())
            {
                int step = _displayedHealth > _owner.GetHealth() ? -1 : 1;
                int targetHealth = _owner.GetHealth();

                yield return new WaitForSeconds(_healthChangeDelay);

                int indexStep = _displayedHealth > _owner.GetHealth() ? 0 : 1;

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