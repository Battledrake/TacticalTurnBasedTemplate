using BattleDrakeCreations.TacticalTurnBasedTemplate;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    [CreateAssetMenu(fileName = "New Blackboard Data", menuName = "Behavior Tree/Blackboard Data")]
    public class BlackboardData : ScriptableObject
    {
        public List<BlackboardEntryData> entries = new();

        public void SetValuesOnBlackboard(Blackboard blackboard)
        {
            foreach(var entry in entries)
            {
                entry.SetValueOnBlackboard(blackboard);
            }
        }
    }

    [Serializable]
    public class BlackboardEntryData : ISerializationCallbackReceiver
    {
        public string keyName;
        public AnyValue.ValueType valueType;
        public AnyValue value;

        public void SetValueOnBlackboard(Blackboard blackboard)
        {
            var key = blackboard.GetOrRegisterKey(keyName);
            setValueDispatchTable[value.type](blackboard, key, value);
        }

        static Dictionary<AnyValue.ValueType, Action<Blackboard, BlackboardKey, AnyValue>> setValueDispatchTable = new()
        {
            {AnyValue.ValueType.Int, (blackboard, key, anyValue) => blackboard.SetValue<int>(key, anyValue) },
            {AnyValue.ValueType.Float, (blackboard, key, anyValue) => blackboard.SetValue<float>(key, anyValue) },
            {AnyValue.ValueType.Bool, (blackboard, key, anyValue) => blackboard.SetValue<bool>(key, anyValue) },
            {AnyValue.ValueType.String, (blackboard, key, anyValue) => blackboard.SetValue<String>(key, anyValue) },
            {AnyValue.ValueType.Vector3, (blackboard, key, anyValue) => blackboard.SetValue<Vector3>(key, anyValue) },
            {AnyValue.ValueType.Transform, (blackboard, key, anyValue) => blackboard.SetValue<Transform>(key, anyValue) },
            {AnyValue.ValueType.GameObject, (blackboard, key, anyValue) => blackboard.SetValue<GameObject>(key, anyValue) },
            {AnyValue.ValueType.Unit, (blackboard, key, anyValue) => blackboard.SetValue<Unit>(key, anyValue) },
            {AnyValue.ValueType.GridIndex, (blackboard, key, anyValue) => blackboard.SetValue<GridIndex>(key, anyValue) },
            {AnyValue.ValueType.Ability, (blackboard, key, anyValue) => blackboard.SetValue<Ability>(key, anyValue) },
        };

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => value.type = valueType;
    }

    [Serializable]
    public struct AnyValue
    {
        public enum ValueType { Int, Float, Bool, String, Vector3, Transform, GameObject, Unit, GridIndex, Ability }
        public ValueType type;

        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public Vector3 vector3Value;
        public Transform transformValue;
        public GameObject gameObjectValue;
        public Unit unitValue;
        public GridIndex gridIndexValue;
        public Ability abilityValue;

        public static implicit operator int(AnyValue value) => value.ConvertValue<int>();
        public static implicit operator float(AnyValue value) => value.ConvertValue<float>();
        public static implicit operator bool(AnyValue value) => value.ConvertValue<bool>();
        public static implicit operator string(AnyValue value) => value.ConvertValue<string>();
        public static implicit operator Vector3(AnyValue value) => value.ConvertValue<Vector3>();
        public static implicit operator Transform(AnyValue value) => value.ConvertValue<Transform>();
        public static implicit operator GameObject(AnyValue value) => value.ConvertValue<GameObject>();
        public static implicit operator Unit(AnyValue value) => value.ConvertValue<Unit>();
        public static implicit operator GridIndex(AnyValue value) => value.ConvertValue<GridIndex>();
        public static implicit operator Ability(AnyValue value) => value.ConvertValue<Ability>();


        T ConvertValue<T>()
        {
            return type switch
            {
                ValueType.Int => AsInt<T>(intValue),
                ValueType.Float => AsFloat<T>(floatValue),
                ValueType.Bool => AsBool<T>(boolValue),
                ValueType.String => (T)(object)stringValue,
                ValueType.Vector3 => (T)(object)vector3Value,
                ValueType.Transform => (T)(object)transformValue,
                ValueType.GameObject => (T)(object)gameObjectValue,
                ValueType.Unit => (T)(object)unitValue,
                ValueType.GridIndex => (T)(object)gridIndexValue,
                ValueType.Ability => (T)(object)abilityValue,
                _ => throw new NotSupportedException($"Not supported value type: {typeof(T)}")
            };
        }

        T AsInt<T>(int value) => typeof(T) == typeof(int) && value is T correctType ? correctType : default;
        T AsFloat<T>(float value) => typeof(T) == typeof(float) && value is T correctType ? correctType : default;
        T AsBool<T>(bool value) => typeof(T) == typeof(bool) && value is T correctType ? correctType : default;
    }
}