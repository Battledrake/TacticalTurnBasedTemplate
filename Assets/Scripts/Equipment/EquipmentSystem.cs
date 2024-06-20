using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public class EquipmentSystem : MonoBehaviour
    {
        public event Action OnActiveWeaponChanged;
        public Weapon ActiveWeapon => _activeWeapon;

        private Dictionary<int, Weapon> _weapons = new();
        private Weapon _activeWeapon;
        private Armor _armor;

        private int _activeWeaponIndex = 0;

        //Effects
        private List<ActiveEffect> _activeWeaponEffects = new();
        private List<ActiveEffect> _activeArmorEffects = new();

        //Sockets
        private Transform _weaponHandSocket;
        private Transform _weaponBackSocket;

        //References
        private AbilitySystem _abilitySystem;
        private Unit _owningUnit;

        private void Awake()
        {
            _owningUnit = this.GetComponent<Unit>();
            _abilitySystem = this.GetComponent<AbilitySystem>();
        }

        public void InitWithStartingEquipment(List<Weapon> startingWeapons, Armor startingArmor = null)
        {
            Debug.Log(_owningUnit);
            _weaponHandSocket = StaticUtilities.FindTransform(_owningUnit.UnitVisual, "WeaponHandSocket");
            _weaponBackSocket = StaticUtilities.FindTransform(_owningUnit.UnitVisual, "WeaponBackSocket");

            if (_weapons.Count > 0)
            {
                foreach (var weapon in _weapons)
                {
                    Destroy(weapon.Value);
                }
            }
            _weapons.Clear();

            if (startingArmor != null)
            {
                EquipArmor(startingArmor);
            }


            Weapon newWeapon = Instantiate(startingWeapons[0], _weaponHandSocket);
            Weapon storedWeapon = Instantiate(startingWeapons[1], _weaponBackSocket);
            _weapons.TryAdd(0, newWeapon);
            _weapons.TryAdd(1, storedWeapon);

            EquipWeapon(_weapons[_activeWeaponIndex]);
        }

        public void EquipWeapon(Weapon weapon)
        {
            _activeWeapon = weapon;

            RemoveEquipmentEffects(_activeWeaponEffects);

            _activeWeaponEffects.Clear();

            _activeWeaponEffects = ApplyEquipmentEffects(_activeWeapon.unitEffects);
        }

        public void EquipArmor(Armor armor)
        {
            _armor = armor;

            RemoveEquipmentEffects(_activeArmorEffects);

            _activeArmorEffects.Clear();

            _activeArmorEffects = ApplyEquipmentEffects(_armor.unitEffects);
        }

        private List<ActiveEffect> ApplyEquipmentEffects(List<GameplayEffect> effects)
        {
            List<ActiveEffect> newActivesList = new();
            for (int i = 0; i < effects.Count; i++)
            {
                ActiveEffect newEffect = _abilitySystem.ApplyEffect(effects[i]);
                newActivesList.Add(newEffect);
            }
            return newActivesList;
        }

        private void RemoveEquipmentEffects(List<ActiveEffect> effects)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                _abilitySystem.RemoveEffect(effects[i]);
            }
        }

        public void SwapWeapons()
        {
            Weapon prevWeapon = _weapons[_activeWeaponIndex];
            prevWeapon.transform.SetParent(_weaponBackSocket, false);
            _activeWeapon = _weapons[_activeWeaponIndex = ++_activeWeaponIndex % _weapons.Count];
            _activeWeapon.transform.SetParent(_weaponHandSocket, false);

            OnActiveWeaponChanged?.Invoke();
        }
    }
}