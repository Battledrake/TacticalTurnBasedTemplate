using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BattleDrakeCreations.BehaviorTree
{
    [CustomEditor(typeof(BlackboardData))]
    public class BlackboardDataEditor : Editor
    {
        private ReorderableList _entryList;

        private void OnEnable()
        {
            _entryList = new ReorderableList(serializedObject, serializedObject.FindProperty("entries"), true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Key");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.3f + 10, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), "Type");
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.6f + 5, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), "Value");
                }
            };

            _entryList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _entryList.serializedProperty.GetArrayElementAtIndex(index);

                rect.y += 2;
                var keyName = element.FindPropertyRelative("keyName");
                var valueType = element.FindPropertyRelative("valueType");
                var value = element.FindPropertyRelative("value");

                var keyNameRect = new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                var valueTypeRect = new Rect(rect.x + rect.width * 0.3f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(keyNameRect, keyName, GUIContent.none);
                EditorGUI.PropertyField(valueTypeRect, valueType, GUIContent.none);

                switch ((AnyValue.ValueType)valueType.enumValueIndex)
                {
                    case AnyValue.ValueType.Int:
                        var intValue = value.FindPropertyRelative("intValue");
                        EditorGUI.PropertyField(valueRect, intValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Float:
                        var floatValue = value.FindPropertyRelative("floatValue");
                        EditorGUI.PropertyField(valueRect, floatValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Bool:
                        var boolValue = value.FindPropertyRelative("boolValue");
                        EditorGUI.PropertyField(valueRect, boolValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.String:
                        var stringValue = value.FindPropertyRelative("stringValue");
                        EditorGUI.PropertyField(valueRect, stringValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Vector3:
                        var vector3Value = value.FindPropertyRelative("vector3Value");
                        EditorGUI.PropertyField(valueRect, vector3Value, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Transform:
                        var transformValue = value.FindPropertyRelative("transformValue");
                        EditorGUI.PropertyField(valueRect, transformValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.GameObject:
                        var gameObjectValue = value.FindPropertyRelative("gameObjectValue");
                        EditorGUI.PropertyField(valueRect, gameObjectValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Unit:
                        var unitValue = value.FindPropertyRelative("unitValue");
                        EditorGUI.PropertyField(valueRect, unitValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.GridIndex:
                        var gridIndexValue = value.FindPropertyRelative("gridIndexValue");
                        EditorGUI.PropertyField(valueRect, gridIndexValue, GUIContent.none);
                        break;
                    case AnyValue.ValueType.Ability:
                        var abilityValue = value.FindPropertyRelative("abilityValue");
                        EditorGUI.PropertyField(valueRect, abilityValue, GUIContent.none);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _entryList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}