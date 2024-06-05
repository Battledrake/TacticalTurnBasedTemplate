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
        [SerializeField] private int _teamIndex = 8;
        [SerializeField] private int _baseActionPoints = 2;
        [SerializeField] private Transform _abilityInstanceContainer;

        public int BaseActionPoints { get => _baseActionPoints; }
        public int CurrentActionPoints { get => _currentActionPoints; }

        public int TeamIndex { get => _teamIndex; set => _teamIndex = value; }

        private Dictionary<AbilityId, Ability> _abilities = new Dictionary<AbilityId, Ability>();
        private Ability _activeAbility;

        private List<AbilityEffectReal> _activeEffects = new List<AbilityEffectReal>();

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

        public void ApplyEffect(AbilityEffectReal effect)
        {
            switch (effect.attributeType)
            {
                case AttributeType.CurrentHealth:
                    _ownerUnit.ModifyCurrentHealth(effect.modifier);
                    break;
                case AttributeType.MaxHealth:
                    break;
                case AttributeType.MoveRange:
                    _ownerUnit.MoveRange += effect.modifier;
                    break;
            }
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
