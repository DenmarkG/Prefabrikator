using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using UnityEditor;
using UnityEngine;

namespace Prefabrikator
{
    [CustomEditor(typeof(ShapeComponent), editorForChildClasses: true)]
    public class PrefabrikatorComponentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit"))
            {
                PrefabrikatorTool.Open(target as ShapeComponent);
            }
            base.OnInspectorGUI();

        }
    }
}