using UnityEngine.UIElements;
using UnityEditor;

namespace BattleDrakeCreations.BehaviorTree
{
    public class InspectorView : VisualElement
    {
        private Editor _editor;

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        public void UpdateSelection(BTNodeView nodeView)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(_editor);

            if (nodeView.Node)
            {
                _editor = Editor.CreateEditor(nodeView.Node);
                IMGUIContainer container = new IMGUIContainer(() =>
                {
                    if (_editor.target)
                        _editor.OnInspectorGUI();
                });
                Add(container);
            }
        }

    }
}
