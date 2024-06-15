using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace BattleDrakeCreations.BehaviorTree
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey>
    {
        private readonly string name;
        private readonly int hashedKey;

        public BlackboardKey(string name)
        {
            this.name = name;
            this.hashedKey = name.ComputeFNV1aHash();
        }

        public bool Equals(BlackboardKey other)
        {
            return this.hashedKey == other.hashedKey;
        }

        public override bool Equals(object obj) => obj is BlackboardKey other && Equals(other);
        public override int GetHashCode() => this.hashedKey;
        public override string ToString() => this.name;

        public static bool operator ==(BlackboardKey lhs, BlackboardKey rhs) => lhs.hashedKey == rhs.hashedKey;
        public static bool operator !=(BlackboardKey lhs, BlackboardKey rhs) => !(lhs == rhs);

    }

    [Serializable]
    public class BlackboardEntry<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        public BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj) => obj is BlackboardEntry<T> other && Key.Equals(other.Key);
        public override int GetHashCode() => Key.GetHashCode();
    }

    public class Blackboard
    {
        public event Action<BlackboardKey> OnValueSet;

        private Dictionary<string, BlackboardKey> _keyRegistry = new();
        private Dictionary<BlackboardKey, object> _entries = new();

        public int EntryCount { get => _entries.Count; }

        public bool AreKeyValueTypesEqual(BlackboardKey key1, BlackboardKey key2)
        {
            bool areSame = false;
            if (_entries.TryGetValue(key1, out var value1) && _entries.TryGetValue(key2, out var value2))
            {
                if (value1.GetType() == value2.GetType())
                {
                    areSame = true;
                }
            }
            return areSame;
        }

        public bool AreValuesEqual<T>(BlackboardKey key1, BlackboardKey key2)
        {
            if (TryGetValue<T>(key1, out var value1) && TryGetValue<T>(key2, out var value2))
            {
                return value1.Equals(value2);
            }
            return false;
        }

        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            if (_entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> castedEntry)
            {
                value = castedEntry.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void Debug()
        {
            foreach (var entry in _entries)
            {
                var entryType = entry.Value.GetType();

                if (entryType.IsGenericType && entryType.GetGenericTypeDefinition() == typeof(BlackboardEntry<>))
                {
                    var valueProperty = entryType.GetProperty("Value");
                    if (valueProperty == null) continue;
                    var value = valueProperty.GetValue(entry.Value);
                    UnityEngine.Debug.Log($"Key: {entry.Key}, Value: {value}");
                }
            }
        }

        public void SetValue<T>(BlackboardKey key, T value)
        {
            _entries[key] = new BlackboardEntry<T>(key, value);
            OnValueSet?.Invoke(key);
        }

        public BlackboardKey GetOrRegisterKey(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return default;

            if (!_keyRegistry.TryGetValue(keyName, out BlackboardKey key))
            {
                key = new BlackboardKey(keyName);
                _keyRegistry[keyName] = key;
            }
            return key;
        }

        public bool ContainsKey(BlackboardKey key) => _entries.ContainsKey(key);
        public void Remove(BlackboardKey key) => _entries.Remove(key);
    }
}