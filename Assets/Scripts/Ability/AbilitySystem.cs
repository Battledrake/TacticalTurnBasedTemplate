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

        private Dictionary<AbilityId, Ability> _abilities = new Dictionary<AbilityId, Ability>();
        private Ability _activeAbility;

        private List<AbilityEffectReal> _activeEffects = new List<AbilityEffectReal>();

        private Unit _owner;

        public void Start()
        {
            _owner = this.transform.GetComponent<Unit>();
            if (_owner)
            {
                List<Ability> unitAbilities = _owner.UnitData.unitStats.abilities;
                for (int i = 0; i < unitAbilities.Count; i++)
                {
                    GiveAbility(unitAbilities[i].GetAbilityId(), unitAbilities[i]);
                }
            }
        }

        private void GiveAbility(AbilityId id, Ability ability)
        {
            if (!_abilities.ContainsKey(id))
            {
                Ability abilityObject = Instantiate(ability, _abilityInstanceContainer);
                ability.SetAbilityOwner(this);
                _abilities.Add(id, abilityObject);
            }
            else
            {
                Debug.Log("AbilitySystem already has an instance of that ability");
            }
        }

        public bool TryActivateAbility(AbilityId id)
        {
            _abilities.TryGetValue(id, out Ability ability);
            return ability.TryActivateAbility();
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
