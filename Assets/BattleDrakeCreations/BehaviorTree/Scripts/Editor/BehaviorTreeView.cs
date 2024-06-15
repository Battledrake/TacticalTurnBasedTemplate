using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BattleDrakeCreations.BehaviorTree
{
    public class BehaviorTreeView : GraphView
    {
        public struct ScriptTemplate
        {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public new class UxmlFactory : UxmlFactory<BehaviorTreeView, UxmlTraits> { }

        public Action<BTNodeView> OnNodeSelected;

        public BehaviorTree Tree { get => _treeAsset; }

        private BehaviorTree _treeAsset;
        private BehaviorTreeSettings _settings;

        public BehaviorTreeView()
        {
            _settings = BehaviorTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet styleSheet = _settings.behaviorTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += Undo_UndORedoPerformed;
        }

        public ScriptTemplate[] scriptFileAssets = {

            new ScriptTemplate{ templateFile=BehaviorTreeSettings.GetOrCreateSettings().scriptTemplateTaskNode, defaultFileName="NewTaskNode.cs", subFolder="Tasks" },
            new ScriptTemplate{ templateFile=BehaviorTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode, defaultFileName="NewCompositeNode.cs", subFolder="Composites" },
            new ScriptTemplate{ templateFile=BehaviorTreeSettings.GetOrCreateSettings().scriptTemplateDecoratorNode, defaultFileName="NewDecoratorNode.cs", subFolder="Decorators" },
        };


        private void Undo_UndORedoPerformed()
        {
            PopulateView(_treeAsset);
            AssetDatabase.SaveAssets();
        }

        public BTNodeView FindNodeView(BTNode node)
        {
            return GetNodeByGuid(node.Guid) as BTNodeView;
        }

        public void PopulateView(BehaviorTree tree)
        {
            graphViewChanged -= OnGraphViewChanged;

            DeleteElements(graphElements);

            if (tree == null)
                return;

            _treeAsset = tree;

            graphViewChanged += OnGraphViewChanged;

            if (tree.RootNode == null)
            {
                EditorApplication.update -= DelayedRootNodeCreation;
                EditorApplication.update += DelayedRootNodeCreation;
                return;
            }

            _treeAsset.Nodes.ForEach(n => CreateNodeView(n));

            //Create edges for nodes
            _treeAsset.Nodes.ForEach(n =>
            {
                List<BTNode> children = _treeAsset.GetChildren(n);
                children.ForEach(c =>
                {
                    BTNodeView parentView = FindNodeView(n);
                    BTNodeView childView = FindNodeView(c);

                    Edge edge = parentView.OutputPort.ConnectTo(childView.InputPort);
                    AddElement(edge);
                });
            });
        }

        //We do this because the asset isn't persistent until naming is finished. This adds the root node after confirmation that the asset is persistent.
        private void DelayedRootNodeCreation()
        {
            if (!AssetDatabase.Contains(_treeAsset))
                return;

            EditorApplication.update -= DelayedRootNodeCreation;


            _treeAsset.RootNode = _treeAsset.CreateNode<RootNode>(Vector2.zero) as RootNode;
            AssetDatabase.SaveAssets();

            PopulateView(_treeAsset);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    BTNodeView nodeView = elem as BTNodeView;
                    if (nodeView != null)
                    {
                        _treeAsset.DeleteNode(nodeView.Node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        BTNodeView parentView = edge.output.node as BTNodeView;
                        BTNodeView childView = edge.input.node as BTNodeView;
                        _treeAsset.RemoveChild(parentView.Node, childView.Node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    BTNodeView parentView = edge.output.node as BTNodeView;
                    BTNodeView childView = edge.input.node as BTNodeView;
                    _treeAsset.AddChild(parentView.Node, childView.Node);
                });
            }

            if (graphViewChange.movedElements != null)
            {
                nodes.ForEach((n) =>
                {
                    BTNodeView view = n as BTNodeView;
                    view.SortChildren();
                });
            }

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction($"Create New.../New Task Node", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create New.../New Composite Node", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create New.../New Decorator Node", (a) => CreateNewScript(scriptFileAssets[2]));

            evt.menu.AppendSeparator();

            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            TypeCache.TypeCollection taskTypes = TypeCache.GetTypesDerivedFrom<TaskNode>();
            foreach (var taskType in taskTypes)
            {
                evt.menu.AppendAction($"Tasks/{taskType.Name}", (a) => CreateNode(taskType, nodePosition));
            }
            TypeCache.TypeCollection compositeTypes = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach (var compositeType in compositeTypes)
            {
                evt.menu.AppendAction($"Composites/{compositeType.Name}", (a) => CreateNode(compositeType, nodePosition));
            }
            TypeCache.TypeCollection decoratorTypes = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
            foreach (var decatorType in decoratorTypes)
            {
                evt.menu.AppendAction($"Decorators/{decatorType.Name}", (a) => CreateNode(decatorType, nodePosition));
            }
        }

        private void SelectFolder(string path)
        {
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }
        private void CreateNewScript(ScriptTemplate template)
        {
            SelectFolder($"{_settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        private void CreateNode(Type type, Vector2 nodePosition)
        {
            BTNode node = _treeAsset.CreateNode(type, nodePosition);
            CreateNodeView(node);
        }

        private void CreateNodeView(BTNode node)
        {
            BTNodeView nodeView = new BTNodeView(node, _settings.nodeXml);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeVisuals()
        {
            nodes.ForEach((n) =>
            {
                BTNodeView nodeView = n as BTNodeView;
                nodeView.UpdateVisual();
            });
        }
    }
}
