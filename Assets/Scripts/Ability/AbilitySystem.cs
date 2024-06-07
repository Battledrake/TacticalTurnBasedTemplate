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


        public Unit GetOwningUnit() => _ownerUnit;
        public GridIndex GetGridIndex() => _ownerUnit ? _ownerUnit.UnitGridIndex : _gridIndex;

        private void Unit_OnTeamIndexChanged()
        {
            _teamIndex = _ownerUnit.TeamIndex;
        }

        public void ResetActionPoints()
        {
            _currentActionPoints = _baseActionPoints;

            UpdateActiveEffectDurationsAndPeriodics();

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
            for (int i = 0; i < attributeData.Count; i++)
            {
                _attributes.TryAdd(attributeData[i].id, attributeData[i]);
                SetAttributeCurrentValue(attributeData[i].id, attributeData[i].baseValue);
            }
        }

        /// <summary>
        /// Returns the base value of an attribute. This value infrequently changes unless Health.
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
            if(attribute == AttributeId.Health)
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
        /// This sets the attributes base value from instant effects. Permanent changes. Current value is set to new value or updated if effects are active.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newValue"></param>
        private void SetAttributeBaseValue(AttributeId id, int newValue)
        {
            if (_attributes.TryGetValue(id, out AttributeData attributeData))
            {
                int oldBase = attributeData.baseValue;

                PreAttributeBaseChanged(id, ref newValue);

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

                //Do we want to keep this callback? We have the new method thing.
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
            }
            else
            {
                Debug.LogWarning($"Attribute {attribute} was not found. Ensure SetAttributeDefaults() is being called and correctly populating the attribute dictionary.");
            }
        }

        //Called on every Action Point reset. Happens on Turn Start.
        private void UpdateActiveEffectDurationsAndPeriodics()
        {
            List<ActiveEffect> effectsToRemove = new List<ActiveEffect>();
            //Go through all our active effects
            foreach (var activeEffectPair in _activeEffects)
            {
                //Go through the list of effects on each attribute.
                for (int i = 0; i < activeEffectPair.Value.Count; i++)
                {
                    //Check that the effect has a duration and is not permanent.
                    if (activeEffectPair.Value[i].DurationPolicy == EffectDurationPolicy.Duration)
                    {
                        //Removes 1 as this is called every turn start.
                        activeEffectPair.Value[i].UpdateDuration(-1);
                        if (activeEffectPair.Value[i].Duration <= 0)
                        {
                            //Add to list for removal after iteration
                            effectsToRemove.Add(activeEffectPair.Value[i]);
                        }
                    }
                    //We don't want to apply a periodic effect to an effect we're removing. Design decision. Contains check could be removed to allow one last periodic effect on removal.
                    if (!effectsToRemove.Contains(activeEffectPair.Value[i]) && activeEffectPair.Value[i].IsPeriodic)
                    {
                        //Update the interval counter. Once it reaches 0 we apply the modifier and reset.
                        activeEffectPair.Value[i].UpdateInterval(-1);
                        if (activeEffectPair.Value[i].CurrentInterval <= 0)
                        {
                            ExecuteEffect(activeEffectPair.Key, activeEffectPair.Value[i].Modifier);
                            activeEffectPair.Value[i].ResetInterval();
                        }
                    }
                }
            }

            for (int i = 0; i < effectsToRemove.Count; i++)
            {
                _activeEffects[effectsToRemove[i].Attribute].Remove(effectsToRemove[i]);

                if (!effectsToRemove[i].IsPeriodic)
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
            if (_activeEffects.TryGetValue(id, out List<ActiveEffect> attributeEffects))
            {
                for (int i = 0; i < attributeEffects.Count; i++)
                {
                    //We don't want periodic effect modifiers to be added to current values. These are applied to base values at intervals.
                    if (!attributeEffects[i].IsPeriodic)
                        effectModifierSum += attributeEffects[i].Modifier;
                }
            }
            int effectSum = baseValue + effectModifierSum;
            SetAttributeCurrentValue(id, effectSum);
        }

        /// <summary>
        /// Executions are called from instant and periodic effects. They modify the base value of an attribute. Permanent stat changes, damage, healing, etc...
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

        /// <summary>
        /// Creates an ActiveEffect from the received effect data and adds it to the active effects. Returns a reference to created ActiveEffect.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        private ActiveEffect ApplyActiveEffectToSelf(AttributeId attribute, AbilityEffectReal effect)
        {
            ActiveEffect newEffect = new ActiveEffect(effect.durationData.durationPolicy, attribute, effect.modifier, effect.durationData.duration, effect.durationData.period.interval);
            if (_activeEffects.TryGetValue(attribute, out List<ActiveEffect> attributeEffects))
            {
                attributeEffects.Add(newEffect);
            }
            else
            {
                List<ActiveEffect> newEffectList = new List<ActiveEffect>() { newEffect };
                _activeEffects.Add(attribute, newEffectList);
            }

            if (newEffect.IsPeriodic)
            {
                if (effect.durationData.period.executeImmediately)
                {
                    ExecuteEffect(attribute, effect.modifier);
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
        /// Bridge for effect application. Instant effects are executed (modifier applied immediately to base value). Duration effects result in an ActiveEffect creation with returned reference.
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        public ActiveEffect ApplyEffect(AbilityEffectReal effect)
        {
            if (effect.durationData.durationPolicy == EffectDurationPolicy.Instant)
            {
                ExecuteEffect(effect.attribute, effect.modifier);
            }
            else
            {
                return ApplyActiveEffectToSelf(effect.attribute, effect);
            }
            return null;
        }

        public bool RemoveEffect(ActiveEffect effectToRemove)
        {
            if (_activeEffects.TryGetValue(effectToRemove.Attribute, out List<ActiveEffect> activeEffects))
            {
                if (activeEffects != null)
                {
                    if (activeEffects.Contains(effectToRemove))
                    {
                        //If it's not a periodic interval application, our current value needs to update to the effect removal.
                        if(!effectToRemove.IsPeriodic)
                        {
                            UpdateAttributeCurrentValue(effectToRemove.Attribute);
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

        public List<Ability> GetAllAbilities()
        {
            return _abilities.Values.ToList();
        }

        public void OnDisable()
        {
            if (_ownerUnit != null)
            {
                _ownerUnit.OnTeamIndexChanged -= Unit_OnTeamIndexChanged;
            }
        }
    }
}
