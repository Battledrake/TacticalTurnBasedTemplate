using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate {
    public class AbilityFactory : MonoBehaviour
    {
        public static AbilityFactory Instance;

        [SerializeField] private List<Ability> _abilityPrefabs;

        private Dictionary<AbilityId, Ability> _abilitiesById = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            DontDestroyOnLoad(this.gameObject);

            for (int i = 0; i < _abilityPrefabs.Count; i++)
            {
                if (_abilitiesById.ContainsKey(_abilityPrefabs[i].GetAbilityId()))
                {
                    Debug.LogWarning($"Duplicate AbilityIds Found. Abilities: {_abilityPrefabs[i].gameObject.name}, {_abilitiesById[_abilityPrefabs[i].GetAbilityId()].gameObject.name}");
                }

                _abilitiesById.TryAdd(_abilityPrefabs[i].GetAbilityId(), _abilityPrefabs[i]);
            }
        }

        public Ability GetNewAbilityInstance(AbilityId abilityId)
        {
            if(_abilitiesById.TryGetValue(abilityId, out Ability ability))
            {
                return SpawnAbility(ability);
            }
            return null;
        }

        private Ability SpawnAbility(Ability ability)
        {
            Ability abilityInstance = Instantiate(ability, this.transform);
            return abilityInstance;
        }
    }
}