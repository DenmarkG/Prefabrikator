using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class NodeGraph : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private VisualElement _root;

        [MenuItem("Prefabrikator/Node View")]
        public static void ShowExample()
        {
            NodeGraph wnd = GetWindow<NodeGraph>();
            wnd.titleContent = new GUIContent("NodeGraph");
        }

        private void OnEnable()
        {
            CreateBlackboard();
        }

        private void CreateBlackboard()
        {
            // pass in the blackboard
            //_root.Query<GraphView>("NodeGraphView")
            var blackboard = new Blackboard();
            blackboard.Add(new BlackboardSection() { title = "Exposed properties" });
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            _root = rootVisualElement;
            m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Code/Editor/GraphView/NodeGraph.uxml");
            m_VisualTreeAsset.CloneTree(_root);

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            //VisualElement label = new Label("Hello World! From C#");
            //root.Add(label);

            // Instantiate UXML
            //VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            //root.Add(labelFromUXML);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Code/Editor/GraphView/NodeGraph.uss");
            _root.styleSheets.Add(styleSheet);
        }
    }
}
