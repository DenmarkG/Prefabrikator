using Prefabrikator.Runtime;
using UnityEditor;
using UnityEngine;

namespace Prefabrikator
{
    [CustomEditor(typeof(PrefabrikatorComponent), editorForChildClasses: true)]
    public class PrefabrikatorComponentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit"))
            {
                PrefabrikatorTool.Open();
            }
            base.OnInspectorGUI();

        }
    }
}