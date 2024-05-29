using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    [CustomEditor(typeof(MovementAbility))]
    public class MovementAbilityEditor : Editor
    {
        SerializedProperty _baseRangeProperty;
        SerializedProperty _areaOfEffectProperty;
        SerializedProperty _areaOfEffectRangeProperty;

        private void OnEnable()
        {
            _baseRangeProperty = serializedObject.FindProperty("_rangeData.rangeMinMax");
            _areaOfEffectProperty = serializedObject.FindProperty("_areaOfEffectData");
            _areaOfEffectRangeProperty = _areaOfEffectProperty.FindPropertyRelative("rangeMinMax");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_areaOfEffectData");

            _areaOfEffectRangeProperty.vector2IntValue = _baseRangeProperty.vector2IntValue * new Vector2Int(0, 2);
            serializedObject.ApplyModifiedProperties();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_areaOfEffectProperty, true);
            EditorGUI.EndDisabledGroup();
        }
    }
}