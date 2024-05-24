using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class AbilitySystem : MonoBehaviour
    {
        private Dictionary<AbilityId, Ability> _abilities = new Dictionary<AbilityId, Ability>();
        private Ability _activeAbility;

        private List<AbilityEffectReal> _activeEffects = new List<AbilityEffectReal>();

        private void Awake()
        {
            Unit unit = this.transform.GetComponent<Unit>();
            if (unit)
            {
                List<Ability> unitAbilities = unit.UnitData.unitStats.abilities;
                for (int i = 0; i < unitAbilities.Count; i++)
                {
                    AddAbility(unitAbilities[i].GetAbilityId(), unitAbilities[i]);
                }
            }
            if (_abilities.Count > 0)
                SetActiveAbility(0);
        }

        private void AddAbility(AbilityId id, Ability ability)
        {
            _abilities.TryAdd(id, ability);
        }

        public void RemoveAbility(AbilityId id, Ability ability)
        {
            _abilities.Remove(id);
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

        public void SetActiveAbility(AbilityId id)
        {
            _activeAbility = _abilities[id];
        }

        public void DisableActiveAbility()
        {
            _activeAbility = null;
        }
    }
}
