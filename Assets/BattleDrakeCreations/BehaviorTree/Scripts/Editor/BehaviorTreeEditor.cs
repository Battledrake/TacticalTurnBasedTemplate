using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace BattleDrakeCreations.BehaviorTree
{
    public class BehaviorTreeEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private StyleSheet m_styleSheet = default;

        private BehaviorTreeView _treeView;
        private BehaviorTree _treeAsset;
        private InspectorView _inspectorView;
        private ToolbarMenu _assetsMenu;
        private TextField _treeNameField;
        private TextField _treeLocationPathField;
        private Button _createNewTreeButton;
        private Button _cancelNewTreeButton;
        private VisualElement _newTreeOverlay;

        [MenuItem("BattleDrakeCreations/Behavior Tree Editor")]
        public static void OpenWindow()
        {
            BehaviorTreeEditor wnd = GetWindow<BehaviorTreeEditor>(typeof(SceneView));
            wnd.titleContent = new GUIContent("Behavior Tree Editor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviorTree)
            {
                OpenWindow();
                return true;
            }
            return false;
        }
        List<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= EditorApplication_OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorApplication_OnPlayModeStateChanged;
        }

        private void EditorApplication_OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            m_VisualTreeAsset.CloneTree(root);

            root.styleSheets.Add(m_styleSheet);

            _treeView = root.Q<BehaviorTreeView>();
            _treeView.OnNodeSelected = OnNodeSelectionChanged;

            _inspectorView = root.Q<InspectorView>();

            _assetsMenu = root.Q<ToolbarMenu>();
            List<BehaviorTree> treeAssets = LoadAssets<BehaviorTree>();
            treeAssets.ForEach(tree =>
            {
                _assetsMenu.menu.AppendAction($"{tree.name}", (a) =>
                {
                    Selection.activeObject = tree;
                });
            });
            _assetsMenu.menu.AppendSeparator();
            _assetsMenu.menu.AppendAction("New Tree...", (a) => DisplayCreateTreeDialog());

            _treeNameField = root.Q<TextField>("TreeName");
            _treeLocationPathField = root.Q<TextField>("LocationPath");
            _newTreeOverlay = root.Q<VisualElement>("Overlay");
            _newTreeOverlay.style.visibility = Visibility.Hidden;

            _createNewTreeButton = root.Q<Button>("CreateButton");
            _createNewTreeButton.clicked += () => CreateNewTree(_treeNameField.value);

            _cancelNewTreeButton = root.Q<Button>("CancelButton");
            _cancelNewTreeButton.clicked += () => HideCreateTreeDialog();

            if (_treeAsset == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(_treeAsset);
            }
        }

        public void DisplayCreateTreeDialog()
        {
            _newTreeOverlay.style.visibility = Visibility.Visible;
        }

        public void HideCreateTreeDialog()
        {
            _newTreeOverlay.style.visibility = Visibility.Hidden;
        }

        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                BehaviorTree tree = Selection.activeObject as BehaviorTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        BehaviorTreeRunner runner = Selection.activeGameObject.GetComponentInChildren<BehaviorTreeRunner>();
                        if (runner)
                        {
                            tree = runner.BehaviorTree;
                        }
                    }
                }
                SelectTree(tree);
            };
        }

        private void SelectTree(BehaviorTree newTree)
        {
            if (_treeView == null)
            {
                return;
            }

            if (!newTree)
            {
                //This populates with a null value, which clears the view. This is done to clear the graph when a tree is deleted
                _treeView.PopulateView(_treeAsset);
                return;
            }

            _treeAsset = newTree;

            _newTreeOverlay.style.visibility = Visibility.Hidden;

            if (Application.isPlaying)
            {
                _treeView.PopulateView(_treeAsset);
            }
            else
            {
                _treeView.PopulateView(_treeAsset);
            }

            EditorApplication.delayCall += () =>
            {
                _treeView.FrameAll();
            };
        }

        private void OnNodeSelectionChanged(BTNodeView NodeView)
        {
            _inspectorView.UpdateSelection(NodeView);
        }

        private void OnInspectorUpdate()
        {
            _treeView?.UpdateNodeVisuals();
        }
        private void CreateNewTree(string assetName)
        {
            string path = System.IO.Path.Combine(_treeLocationPathField.value, $"{assetName}.asset");
            BehaviorTree tree = CreateInstance<BehaviorTree>();
            tree.name = _treeNameField.ToString();
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
        }
    }
}
