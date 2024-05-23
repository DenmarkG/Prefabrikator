using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Prefabrikator.Editor
{
    public class NodeGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<NodeGraphView, UxmlTraits> { }
        public NodeGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());


            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Code/Editor/GraphView/NodeGraph.uss");
            styleSheets.Add(styleSheet);

            graphViewChanged += OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is Node)
                    {
                        Debug.Log("Node deleted");
                    }

                    if (element is Edge)
                    {
                        Debug.Log("Edge deleted");
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    //
                }
            }

            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Validate node directions and connections to prevent loops
            var list = ports.ToList();
            for (int i = list.Count - 1; i >= 0; --i)
            {
                var endPort = list[i];
                if (endPort != null)
                {
                    if (endPort.direction == startPort.direction || endPort.node == startPort.node)
                    {
                        list.RemoveAt(i);
                    }
                }
                else
                {
                    list.RemoveAt(i);
                }
            }

            return list;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 cachedLocalPosition = evt.localMousePosition;
            IEventHandler currentTarget = evt.currentTarget;

            evt.menu.AppendAction("Add Node", (menuAction) => CreateNode(cachedLocalPosition));
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Delete Node", (menuAction) => DeleteNode(currentTarget));
        }

        private void DeleteNode(IEventHandler currentTarget)
        {
            if (currentTarget is NodeGraphView graphView)
            {
                for (int i = graphView.selection.Count - 1; i >= 0; --i)
                {
                    RemoveElement(graphView.selection[i] as GraphElement);
                }
            }
        }

        private void CreateNode(Vector2 position)
        {
            // Pass in and set position from event
            NodeView node = new("New node", position);
            AddElement(node);
        }
    }
}