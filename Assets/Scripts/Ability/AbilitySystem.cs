using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilitySystem : MonoBehaviour
    {
        [SerializeField] private Transform _abilityInstanceContainer;

        [SerializeField] private int _baseAbilityPoints = 2;

        public int BaseAbilityPoints { get => _baseAbilityPoints; }
        public int CurrentAbilityPoints { get => _currentAbilityPoints; }

        private Dictionary<AbilityId, Ability> _abilities = new Dictionary<AbilityId, Ability>();
        private Ability _activeAbility;

        private List<AbilityEffectReal> _activeEffects = new List<AbilityEffectReal>();

        private int _currentAbilityPoints;
        private Unit _ownerUnit;

        public Unit GetOwningUnit() { return _ownerUnit; }

        public void ResetAbilityPoints()
        {
            _currentAbilityPoints = _baseAbilityPoints;
        }
        public void AddAbilityPoints(int amount)
        {
            _currentAbilityPoints += amount;
        }

        public void RemoveAbilityPoints(int amount)
        {
            _currentAbilityPoints -= amount;
        }

        public void InitAbilitySystem(Unit owner, List<Ability> abilities)
        {
            if (owner != null)
            {
                _ownerUnit = owner;

                for (int i = 0; i < abilities.Count; i++)
                {
                    GiveAbility(abilities[i].GetAbilityId(), abilities[i]);
                }
            }
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

        public void UpdateEffectDurations()
        {
            //for(int i = 0; i < _activeEffects.Count; i++)
            //{
            //    if(_activeEffects.EffectType == EffectType.Cooldown)
            //    {
            //        _activeEffects[i].duration -= 1;
            //    }
            //}
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
    }
}
