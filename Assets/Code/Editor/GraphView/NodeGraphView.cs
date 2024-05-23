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

            // Figure out search provider to replace this; check out game dev guid search window video
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<NodeView>();
            foreach (Type type in types)
            {
                evt.menu.AppendAction($"Add {type.Name}", (menuAction) => CreateNode(type, cachedLocalPosition));
            }
            

            
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

        private void CreateNode(System.Type type, Vector2 position)
        {
            var node = Activator.CreateInstance(type, new object[] { position }) as NodeView;
            //NodeView node = new("New node", position);
            AddElement(node);
        }
    }
}