using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class NodeGraph : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Prefabrikator/Node View")]
        public static void ShowExample()
        {
            NodeGraph wnd = GetWindow<NodeGraph>();
            wnd.titleContent = new GUIContent("NodeGraph");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Code/Editor/GraphView/NodeGraph.uxml");
            m_VisualTreeAsset.CloneTree(root);

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            //VisualElement label = new Label("Hello World! From C#");
            //root.Add(label);

            // Instantiate UXML
            //VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            //root.Add(labelFromUXML);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Code/Editor/GraphView/NodeGraph.uss");
            root.styleSheets.Add(styleSheet);
        }
    }
}
