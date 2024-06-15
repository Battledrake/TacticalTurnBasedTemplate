using UnityEditor;
using UnityEngine.UIElements;

namespace BattleDrakeCreations.BehaviorTree
{
    public class DoubleClickSelection : MouseManipulator
    {
        private double _timeElapsed;
        private double _doubleClickDuration = 0.3;

        public DoubleClickSelection()
        {
            _timeElapsed = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            BehaviorTreeView btGraphView = target as BehaviorTreeView;
            if (btGraphView == null)
                return;

            double duration = EditorApplication.timeSinceStartup - _timeElapsed;
            if (duration < _doubleClickDuration)
            {
                SelectChildren(evt);
            }

            _timeElapsed = EditorApplication.timeSinceStartup;
        }

        void SelectChildren(MouseDownEvent evt)
        {
            BehaviorTreeView btGraphView = target as BehaviorTreeView;
            if (btGraphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            BTNodeView clickedElement = evt.target as BTNodeView;
            if (clickedElement == null)
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<BTNodeView>();
                if (clickedElement == null)
                    return;
            }

            btGraphView.Tree.Traverse(clickedElement.Node, (node) =>
            {
                BTNodeView nodeView = btGraphView.FindNodeView(node);
                btGraphView.AddToSelection(nodeView);
            });
        }
    }
}