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
        private Dictionary<AttributeId, List<ActiveEffect>> _activeEffects = new Dictionary<AttributeId, List<ActiveEffect>>();

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

            UpdateActiveEffectDurations();

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
                _attributes.TryAdd(attributeData[i].id, attributeData[i]);
                SetAttributeCurrentValue(attributeData[i].id, attributeData[i].baseValue);
            }
        }

        /// <summary>
        /// Returns the base value of an attribute. This value infrequently changes unless Health.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetAttributeBaseValue(AttributeId id)
        {
            return _attributes[id].baseValue;
        }

        /// <summary>
        /// Return the current value of an attribute. Includes temporary effects like buffs/debuffs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetAttributeCurrentValue(AttributeId id)
        {
            return _attributes[id].GetCurrentValue();
        }

        /// <summary>
        /// This sets the attributes base value from instant effects. Permanent changes. Current value is set to new value or updated if effects are active.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newValue"></param>
        private void SetAttributeBaseValue(AttributeId id, int newValue)
        {
            if(_attributes.TryGetValue(id, out AttributeData attributeData))
            {
                int oldBase = attributeData.baseValue;
                attributeData.baseValue = newValue;
                _attributes[id] = attributeData;

                if (_activeEffects.ContainsKey(id) && _activeEffects[id].Count > 0)
                {
                    UpdateAttributeCurrentValue(id);
                }
                else
                {
                    SetAttributeCurrentValue(id, newValue);
                }

                OnAttributeBaseChanged?.Invoke(id, oldBase, newValue);
            }
            else
            {
                Debug.LogWarning($"Attribute {id.ToString()} was not found. Ensure SetAttributeDefaults() is being called and correctly populating the attribute dictionary.");
            }
        }

        /// <summary>
        /// This sets the current value. Any existing duration effects should have been accounted for by the caller.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newValue"></param>
        private void SetAttributeCurrentValue(AttributeId id, int newValue)
        {
            if(_attributes.TryGetValue(id, out AttributeData attributeData))
            {
                int oldCurrent = attributeData.GetCurrentValue();
                attributeData.SetCurrentValue(newValue);
                _attributes[id] = attributeData;

                OnAttributeCurrentChanged?.Invoke(id, oldCurrent, newValue);
            }
            else
            {
                Debug.LogWarning($"Attribute {id.ToString()} was not found. Ensure SetAttributeDefaults() is being called and correctly populating the attribute dictionary.");
            }
        }

        private void ApplyPeriodicEffects()
        {
            //We'll use this for effects with periodic applications like dots/hots. Iterate over active effects, check their period policy, check if expired, apply modifier.
            //Need to decide if we want this to occur at turn start before effect removals? After? Turn ends? Decisions decisions.
        }

        //We're going to want to bind this to an on TurnStarted call.
        public void UpdateActiveEffectDurations()
        {
            List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();
            foreach(var activeEffectPair in _activeEffects)
            {
                for(int i = 0; i < activeEffectPair.Value.Count; i++)
                {
                    if (activeEffectPair.Value[i].DurationPolicy == EffectDurationPolicy.Duration)
                    {
                        activeEffectPair.Value[i].UpdateDuration(-1);
                        if (activeEffectPair.Value[i].Duration <= 0)
                        {
                            effectsToRemove.Add(activeEffectPair.Value[i]);
                        }
                    }
                }
            }

            for(int i = 0; i < effectsToRemove.Count; i++)
            {
                _activeEffects[effectsToRemove[i].Attribute].Remove(effectsToRemove[i]);
                UpdateAttributeCurrentValue(effectsToRemove[i].Attribute);
            }
        }

        /// <summary>
        /// We use this to calculate the current attribute value based on the base value + any duration effects
        /// </summary>
        /// <param name="id"></param>
        private void UpdateAttributeCurrentValue(AttributeId id)
        {
            int baseValue = _attributes[id].baseValue;
            int effectModifierSum = 0;
            if(_activeEffects.TryGetValue(id, out List<ActiveEffect> attributeEffects))
            {
                for (int i = 0; i < attributeEffects.Count; i++)
                {
                    effectModifierSum += attributeEffects[i].Modifier;
                }
            }
            int effectSum = baseValue + effectModifierSum;
            SetAttributeCurrentValue(id, effectSum);
        }

        /// <summary>
        /// Executions are called from instant effects. They modify the base value of an attribute.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="effect"></param>
        private void ExecuteEffect(AttributeId id, int modifier)
        {
            int baseValue = _attributes[id].baseValue;
            int newValue = baseValue + modifier;
            SetAttributeBaseValue(id, newValue);
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

        private void AddEffectToActiveEffects(AttributeId attribute, AbilityEffectReal effect)
        {
            ActiveEffect newEffect = new ActiveEffect(effect.duration.durationPolicy, attribute, effect.modifier, effect.duration.modifier);
            if(_activeEffects.TryGetValue(attribute, out List<ActiveEffect> attributeEffects))
            {
                attributeEffects.Add(newEffect);
            }
            else
            {
                List<ActiveEffect> newEffectList = new List<ActiveEffect>() { newEffect };
                _activeEffects.Add(attribute, newEffectList);
            }
        }

        public bool TryActivateAbility(AbilityId id, AbilityActivationData activationData)
        {
            _abilities.TryGetValue(id, out Ability ability);
            return ability.TryActivateAbility(activationData);
        }

        public void ApplyEffects(List<AbilityEffectReal> effects)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                ApplyEffect(effects[i]);
            }
        }

        public void ApplyEffect(AbilityEffectReal effect)
        {
            //If it's an instant effect, we modify the base value of the attribute.
            if(effect.duration.durationPolicy == EffectDurationPolicy.Instant)
            {
                ExecuteEffect(effect.attribute, effect.modifier);
            }
            else
            {
                //Here we want to apply the effect. This will modify the current value and add the effect to an activeEffects container.
                AddEffectToActiveEffects(effect.attribute, effect);
                UpdateAttributeCurrentValue(effect.attribute);
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
