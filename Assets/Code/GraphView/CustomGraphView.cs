using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CustomGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<CustomGraphView, UxmlTraits> { }
    public CustomGraphView()
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());


        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Code/GraphView/PrefabrikatorGraph.uss");
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
                    // Remove node
                }

                if (element is Edge)
                {
                    // Remove edge
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
        evt.menu.AppendAction("Add Node", (menuAction) => CreateNode() );
    }

    private void CreateNode()
    {
        // Pass in and set position from event
        CustomNodeView node = new("New node");
        AddElement(node);
    }
}
