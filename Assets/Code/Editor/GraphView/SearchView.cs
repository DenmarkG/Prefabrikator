using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class SearchView : ScriptableObject, ISearchWindowProvider
    {
        private NodeGraphView _graphView = null;
        private EditorWindow _window = null;

        public void Init(NodeGraphView graphView, EditorWindow window)
        {
            _graphView = graphView;
            _window = window;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Vector"), 1),
                new SearchTreeEntry(new GUIContent("Node"))
                {
                    userData = new Vector3Node(context.screenMousePosition), level = 2
                },
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var mouseWorldPosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, 
                context.screenMousePosition - _window.position.position);

            var localMosePos = _graphView.WorldToLocal(mouseWorldPosition);
            // create the in the graph view here
            return true;
        }
    }
}