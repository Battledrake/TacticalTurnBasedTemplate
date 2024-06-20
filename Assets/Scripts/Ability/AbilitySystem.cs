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

        [SerializeField] private Transform _abilityInstanceContainer;
        [SerializeField] private GameplayEffectsContainer _actionPointEffect;
        public Unit OwningUnit => _owningUnit;

        private Dictionary<AttributeId, AttributeData> _attributes = new Dictionary<AttributeId, AttributeData>();
        private Dictionary<AbilityId, Ability> _abilities = new Dictionary<AbilityId, Ability>();
        private Dictionary<AttributeId, List<ActiveEffect>> _activeEffects = new Dictionary<AttributeId, List<ActiveEffect>>();
        private Unit _owningUnit;

        public void ResetActionPoints()
        {
            SetAttributeBaseValue(AttributeId.ActionPoints, 0);

            UpdateActiveEffectDurationsAndPeriodics();

            foreach (KeyValuePair<AbilityId, Ability> abilityPair in _abilities)
            {
                abilityPair.Value.ReduceCooldown(-1);
            }
        }

        public void InitAbilitySystem(Unit owner, List<AttributeData> attributeSet, List<AbilityId> startingAbilities)
        {
            if (owner != null)
            {
                _owningUnit = owner;
            }

            _activeEffects.Clear();

            if (attributeSet != null)
                SetAttributeDefaults(attributeSet);

            if (_abilities.Count > 0)
            {
                foreach (KeyValuePair<AbilityId, Ability> abilityPair in _abilities)
                {
                    Destroy(abilityPair.Value.gameObject);
                }
                _abilities.Clear();
            }

            if (startingAbilities != null)
            {
                for (int i = 0; i < startingAbilities.Count; i++)
                {
                    AddAbility(startingAbilities[i]);
                }
            }

            if (_actionPointEffect != null)
            {
                ApplyEffect(_actionPointEffect.effects[0]);
            }
        }

        private void SetAttributeDefaults(List<AttributeData> attributeData)
        {
            _attributes.Clear();
            //First we populate the dictionary with attributes and base values.
            for (int i = 0; i < attributeData.Count; i++)
            {
                _attributes.TryAdd(attributeData[i].attribute, attributeData[i]);
            }
            //We then set current values in a separate loop. This is done separately to ensure dictionary is populated for any attribute checks on PreAttributeCurrentChanges.
            for(int i = 0; i < _attributes.Count; i++)
            {
                SetAttributeCurrentValue(attributeData[i].attribute, attributeData[i].baseValue);
            }

        }

        /// <summary>
        /// Returns the base value of an attribute. This is the value before any effects are applied to it.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public int GetAttributeBaseValue(AttributeId attribute)
        {
            return _attributes[attribute].baseValue;
        }

        /// <summary>
        /// Return the current value of an attribute. Includes temporary effects like buffs/debuffs.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public int GetAttributeCurrentValue(AttributeId attribute)
        {
            return _attributes[attribute].GetCurrentValue();
        }


        /// <summary>
        /// This is called right before the new base value is set. Use this to clamp or make any needed modifications to an attribute's base value before it's set.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="newValue"></param>
        private void PreAttributeBaseChanged(AttributeId attribute, ref int newValue)
        {
            if (attribute == AttributeId.Health)
            {
                newValue = Mathf.Clamp(newValue, 0, GetAttributeCurrentValue(AttributeId.MaxHealth));
            }
        }

        /// <summary>
        /// This is called right before the new current value is set. Use this to clamp or make any needed modifications to an attribute's current value before it's set.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="newValue"></param>
        private void PreAttributeCurrentChanged(AttributeId attribute, ref int newValue)
        {

        }

        /// <summary>
        /// This is called after the new base is set. You can use this to apply changes to other attributes based on this change.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="oldBase"></param>
        /// <param name="newValue"></param>
        private void PostAttributeBaseChanged(AttributeId attribute, int oldBase, int newValue)
        {
        }

        /// <summary>
        /// This is called after the new current is set. You can use this to apply changes to other attributes based on this change like setting health to new max or changing other attributes on a level up.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="oldCurrent"></param>
        /// <param name="newValue"></param>
        private void PostAttributeCurrentChanged(AttributeId attribute, int oldCurrent, int newValue)
        {
            if (attribute == AttributeId.MaxHealth)
            {
                SetAttributeBaseValue(AttributeId.Health, newValue);
            }
        }

        /// <summary>
        /// This sets the attributes base value from instant and periodic effects. Permanent changes. Current value is also set to new value or updated if effects are active.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="newValue"></param>
        private void SetAttributeBaseValue(AttributeId attribute, int newValue)
        {
            if (_attributes.TryGetValue(attribute, out AttributeData attributeData))
            {
                int oldBase = attributeData.baseValue;

                PreAttributeBaseChanged(attribute, ref newValue);

                attributeData.baseValue = newValue;
                _attributes[attribute] = attributeData;

                if (_activeEffects.ContainsKey(attribute) && _activeEffects[attribute].Count > 0)
                {
                    UpdateAttributeCurrentValue(attribute);
                }
                else
                {
                    SetAttributeCurrentValue(attribute, newValue);
                }

                //Do we want to keep this callback? We have the new method thing.
                OnAttributeBaseChanged?.Invoke(attribute, oldBase, newValue);

                PostAttributeBaseChanged(attribute, oldBase, newValue);
            }
            else
            {
                Debug.LogWarning($"Attribute {attribute.ToString()} was not found. Ensure SetAttributeDefaults() is being called and correctly populating the attribute dictionary.");
            }
        }

        /// <summary>
        /// This sets the current value. Any existing effect magnitudes should have been accounted for by the caller.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="newValue"></param>
        private void SetAttributeCurrentValue(AttributeId attribute, int newValue)
        {
            if (_attributes.TryGetValue(attribute, out AttributeData attributeData))
            {
                int oldCurrent = attributeData.GetCurrentValue();

                PreAttributeCurrentChanged(attribute, ref newValue);

                attributeData.SetCurrentValue(newValue);
                _attributes[attribute] = attributeData;

                OnAttributeCurrentChanged?.Invoke(attribute, oldCurrent, newValue);

                PostAttributeCurrentChanged(attribute, oldCurrent, newValue);
            }
            else
            {
                Debug.LogWarning($"Attribute {attribute} was not found. Ensure SetAttributeDefaults() is being called and correctly populating the attribute dictionary.");
            }
        }

        /// <summary>
        /// Cycles through all active effects on every turn start. Durations are checked and updated. Expired effects removed. Periodic effects are executed.
        /// </summary>
        private void UpdateActiveEffectDurationsAndPeriodics()
        {
            List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();
            //Go through all our active effects
            foreach (KeyValuePair<AttributeId, List<ActiveEffect>> activeEffectPair in _activeEffects)
            {
                //Go through the list of effects on each attribute.
                for (int i = 0; i < activeEffectPair.Value.Count; i++)
                {
                    //Check that the effect has a duration and is not permanent.
                    if (activeEffectPair.Value[i].durationPolicy == EffectDurationPolicy.Duration)
                    {
                        //Removes 1 as this is called every turn start.
                        activeEffectPair.Value[i].UpdateDuration(-1);
                        if (activeEffectPair.Value[i].duration <= 0)
                        {
                            //Add to list for removal after iteration
                            effectsToRemove.Add(activeEffectPair.Value[i]);
                        }
                    }
                    //We don't want to apply a periodic effect to an effect we're removing. Design decision. Contains check could be removed to allow one last periodic effect on removal.
                    if (!effectsToRemove.Contains(activeEffectPair.Value[i]) && activeEffectPair.Value[i].isPeriodic)
                    {
                        //Update the interval counter. Once it reaches 0 we apply the magnitude and reset.
                        activeEffectPair.Value[i].UpdateInterval(-1);
                        if (activeEffectPair.Value[i].currentInterval <= 0)
                        {
                            ExecuteEffect(activeEffectPair.Key, activeEffectPair.Value[i].magnitude);
                            activeEffectPair.Value[i].ResetInterval();
                        }
                    }
                }
            }

            for (int i = 0; i < effectsToRemove.Count; i++)
            {
                _activeEffects[effectsToRemove[i].attribute].Remove(effectsToRemove[i]);

                if (!effectsToRemove[i].isPeriodic)
                    UpdateAttributeCurrentValue(effectsToRemove[i].attribute);
            }
        }

        /// <summary>
        /// We use this to calculate the current attribute value based on the base value + any duration effects
        /// </summary>
        /// <param name="id"></param>
        private void UpdateAttributeCurrentValue(AttributeId id)
        {
            int baseValue = _attributes[id].baseValue;
            int effectMagnitudeSum = 0;
            if (_activeEffects.TryGetValue(id, out List<ActiveEffect> attributeEffects))
            {
                for (int i = 0; i < attributeEffects.Count; i++)
                {
                    //We don't want periodic effect magnitudes to be added to current values. These are applied to base values at intervals.
                    if (!attributeEffects[i].isPeriodic)
                        effectMagnitudeSum += attributeEffects[i].magnitude;
                }
            }
            int effectSum = baseValue + effectMagnitudeSum;
            SetAttributeCurrentValue(id, effectSum);
        }

        /// <summary>
        /// Executions are called from instant and periodic effects. They modify the base value of an attribute. Permanent stat changes, damage, healing, etc...
        /// </summary>
        /// <param name="id"></param>
        /// <param name="effect"></param>
        private void ExecuteEffect(AttributeId id, int magnitude)
        {
            int baseValue = _attributes[id].baseValue;
            int newValue = baseValue + magnitude;
            SetAttributeBaseValue(id, newValue);
        }

        public void AddAbility(AbilityId id)
        {
            if (!_abilities.ContainsKey(id))
            {
                Ability abilityInstance = AbilityFactory.Instance.GetNewAbilityInstance(id);
                if(abilityInstance != null)
                {
                    abilityInstance.transform.SetParent(_abilityInstanceContainer);
                    abilityInstance.InitAbility(this);
                    _abilities.Add(id, abilityInstance);
                }
            }
            else
            {
                Debug.Log("AbilitySystem already has an instance of that ability");
            }
        }

        /// <summary>
        /// Creates an ActiveEffect from the received effect data and adds it to the active effects. Returns a reference to created ActiveEffect.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        private ActiveEffect ApplyActiveEffectToSelf(AttributeId attribute, GameplayEffect effect)
        {
            ActiveEffect newEffect = new ActiveEffect(effect.durationData.durationPolicy, attribute, effect.magnitude, effect.durationData.duration, effect.durationData.period.interval);
            if (_activeEffects.TryGetValue(attribute, out List<ActiveEffect> attributeEffects))
            {
                attributeEffects.Add(newEffect);
            }
            else
            {
                List<ActiveEffect> newEffectList = new List<ActiveEffect>() { newEffect };
                _activeEffects.Add(attribute, newEffectList);
            }

            if (newEffect.isPeriodic)
            {
                if (effect.durationData.period.executeImmediately)
                {
                    ExecuteEffect(attribute, effect.magnitude);
                }
            }
            else
            {
                UpdateAttributeCurrentValue(effect.attribute);
            }

            return newEffect;
        }

        public bool TryActivateAbility(AbilityId id, AbilityActivationData activationData)
        {
            _abilities.TryGetValue(id, out Ability ability);
            return ability.TryActivateAbility(activationData);
        }

        /// <summary>
        /// Bridge for effect application. Instant effects are executed (magnitude applied immediately to base value). Duration effects result in an ActiveEffect creation with returned reference.
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public ActiveEffect ApplyEffect(GameplayEffect effect)
        {
            if (effect.durationData.durationPolicy == EffectDurationPolicy.Instant)
            {
                ExecuteEffect(effect.attribute, effect.magnitude);
            }
            else
            {
                return ApplyActiveEffectToSelf(effect.attribute, effect);
            }
            return null;
        }

        public bool RemoveEffect(ActiveEffect effectToRemove)
        {
            if (_activeEffects.TryGetValue(effectToRemove.attribute, out List<ActiveEffect> activeEffects))
            {
                if (activeEffects != null)
                {
                    if (activeEffects.Contains(effectToRemove))
                    {
                        //If it's not a periodic interval application, our current value needs to update to the effect removal.
                        if (!effectToRemove.isPeriodic)
                        {
                            UpdateAttributeCurrentValue(effectToRemove.attribute);
                        }
                        activeEffects.Remove(effectToRemove);
                        return true;
                    }
                }
            }
            return false;
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

        public List<Ability> GetAbilities()
        {
            return _abilities.Values.ToList();
        }
    }
}
