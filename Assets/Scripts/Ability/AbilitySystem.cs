using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilitySystem : MonoBehaviour
    {
        public event Action<AttributeId, int, int> OnAttributeBaseChanged;
        public event Action<AttributeId, int, int> OnAttributeCurrentChanged;

        [SerializeField] private int _teamIndex = 8;
        [SerializeField] private int _baseActionPoints = 2;
        [SerializeField] private Transform _abilityInstanceContainer;

        public int BaseActionPoints { get => _baseActionPoints; }
        public int CurrentActionPoints { get => _currentActionPoints; }

        public int TeamIndex { get => _teamIndex; set => _teamIndex = value; }


        private Dictionary<AttributeId, AttributeData> _attributes = new Dictionary<AttributeId, AttributeData>();
        private Dictionary<AbilityId, Ability> _abilities = new Dictionary<AbilityId, Ability>();
        private Ability _activeAbility;


        private int _currentActionPoints;
        private Unit _ownerUnit;
        private GridIndex _gridIndex = new GridIndex(0, 0);


        public Unit GetOwningUnit() { return _ownerUnit; }

        public GridIndex GetGridIndex()
        {
            return _ownerUnit ? _ownerUnit.UnitGridIndex : _gridIndex;
        }

        private void Unit_OnTeamIndexChanged()
        {
            _teamIndex = _ownerUnit.TeamIndex;
        }

        public void ResetActionPoints()
        {
            _currentActionPoints = _baseActionPoints;

            foreach (var abilityPair in _abilities)
            {
                abilityPair.Value.ReduceCooldown(1);
            }
        }
        public void AddActionPoints(int amount)
        {
            _currentActionPoints += amount;
        }

        public void RemoveActionPoints(int amount)
        {
            _currentActionPoints -= amount;
        }

        public void InitAbilitySystem(Unit owner, List<Ability> abilities)
        {
            //TODO: Continue work to separate Unit logic away from this system entirely. IAbilitySystem interface or something.
            if (owner != null)
            {
                _ownerUnit = owner;
                _ownerUnit.OnTeamIndexChanged += Unit_OnTeamIndexChanged;
            }

            _currentActionPoints = _baseActionPoints;

            for (int i = 0; i < abilities.Count; i++)
            {
                GiveAbility(abilities[i].GetAbilityId(), abilities[i]);
            }
        }

        public void SetAttributeDefaults(List<AttributeData> attributeData)
        {
            _attributes.Clear();
            for(int i = 0; i < attributeData.Count; i++)
            {
                AttributeData newAttribute = attributeData[i];
                int oldCurrent = newAttribute.GetCurrentValue();
                newAttribute.SetCurrentValue(newAttribute.baseValue);
                _attributes.TryAdd(newAttribute.id, newAttribute);
                OnAttributeCurrentChanged?.Invoke(newAttribute.id, oldCurrent, newAttribute.GetCurrentValue());
            }
        }

        public int GetAttributeBaseValue(AttributeId id)
        {
            return _attributes[id].baseValue;
        }

        public int GetAttributeCurrentValue(AttributeId id)
        {
            return _attributes[id].GetCurrentValue();
        }

        private void SetAttributeBaseValue(AttributeId id, int newValue)
        {
            AttributeData modifiedData = _attributes[id];
            int oldBase = modifiedData.baseValue;
            modifiedData.baseValue = newValue;
            _attributes[id] = modifiedData;
            OnAttributeBaseChanged?.Invoke(id, oldBase, newValue);

            //TODO: if(_durationEffects.Contains(effect.attribute == id), get that value, add to base.
            SetAttributeCurrentValue(id, newValue);
        }

        private void ApplyModToAttribute(AttributeId id, int modifier)
        {
            int newBase = _attributes[id].baseValue + modifier;
            SetAttributeBaseValue(id, newBase);
        }

        private void SetAttributeCurrentValue(AttributeId id, int newValue)
        {
            AttributeData modifiedData = _attributes[id];
            int oldCurrent = modifiedData.GetCurrentValue();
            modifiedData.SetCurrentValue(modifiedData.baseValue);
            _attributes[id] = modifiedData;
            OnAttributeCurrentChanged?.Invoke(id, oldCurrent, newValue);
        }

        private void GiveAbility(AbilityId id, Ability ability)
        {
            if (!_abilities.ContainsKey(id))
            {
                Ability abilityObject = Instantiate(ability, _abilityInstanceContainer);
                abilityObject.InitAbility(this);
                _abilities.Add(id, abilityObject);
            }
            else
            {
                Debug.Log("AbilitySystem already has an instance of that ability");
            }
        }

        public bool TryActivateAbility(AbilityId id, AbilityActivationData activationData)
        {
            _abilities.TryGetValue(id, out Ability ability);
            return ability.TryActivateAbility(activationData);
        }

        public void ApplyEffect(AbilityEffectReal effect)
        {
            ApplyModToAttribute(effect.attribute, effect.modifier);
        }

        public void ApplyEffects(List<AbilityEffectReal> effects)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ApplyEffect(effects[i]);
            }
        }

        public void RemoveAbility(AbilityId id)
        {
            _abilities.TryGetValue(id, out Ability abilityToRemove);
            _abilities.Remove(id);
            Destroy(abilityToRemove.gameObject);
        }

        public Ability GetAbility(AbilityId id)
        {
            if (_abilities.ContainsKey(id))
                return _abilities[id];
            else
                return null;
        }

        public List<Ability> GetAllAbilities()
        {
            return _abilities.Values.ToList();
        }

        public void OnDisable()
        {
            if(_ownerUnit != null)
            {
                _ownerUnit.OnTeamIndexChanged -= Unit_OnTeamIndexChanged;
            }
        }
    }
}
