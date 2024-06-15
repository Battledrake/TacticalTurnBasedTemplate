using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BattleDrakeCreations.BehaviorTree
{
    public class BehaviorTreeSettings : ScriptableObject
    {
        public VisualTreeAsset behaviorTreeXml;
        public StyleSheet behaviorTreeStyle;
        public VisualTreeAsset nodeXml;
        public TextAsset scriptTemplateTaskNode;
        public TextAsset scriptTemplateCompositeNode;
        public TextAsset scriptTemplateDecoratorNode;
        public string newNodeBasePath = "Assets/BattleDrakeCreations/BehaviorTree/Scripts";

        public static BehaviorTreeSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:BehaviorTreeSettings");
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Found multiple settings files, using the first.");
            }

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<BehaviorTreeSettings>(path);
            }
        }

        public static BehaviorTreeSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = CreateInstance<BehaviorTreeSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/BattleDrakeCreations/BehaviorTree/BehaviorTreeSettings.asset");
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using UIElements for the drawing framework:
    static class MyCustomSettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/BehaviorTreeUIElementsSettings", SettingsScope.Project)
            {
                label = "BehaviorTree",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = BehaviorTreeSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var title = new Label()
                    {
                        text = "Behavior Tree Settings"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new InspectorElement(settings));

                    rootElement.Bind(settings);
                },
            };

            return provider;
        }
    }
}