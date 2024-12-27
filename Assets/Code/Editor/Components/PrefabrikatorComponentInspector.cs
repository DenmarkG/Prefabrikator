using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using UnityEditor;
using UnityEngine;

namespace Prefabrikator
{
    [CustomEditor(typeof(CustomShape), editorForChildClasses: true)]
    public class PrefabrikatorComponentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit"))
            {
                PrefabrikatorTool.Open(target as CustomShape);
            }
            base.OnInspectorGUI();

        }
    }
}