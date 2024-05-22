using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PrefabrikatorGraph : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/PrefabrikatorGraph")]
    public static void ShowExample()
    {
        PrefabrikatorGraph wnd = GetWindow<PrefabrikatorGraph>();
        wnd.titleContent = new GUIContent("PrefabrikatorGraph");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Code/GraphView/PrefabrikatorGraph.uxml");
        m_VisualTreeAsset.CloneTree(root);

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        //root.Add(label);

        // Instantiate UXML
        //VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        //root.Add(labelFromUXML);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Code/GraphView/PrefabrikatorGraph.uss");
        root.styleSheets.Add(styleSheet);
    }
}
