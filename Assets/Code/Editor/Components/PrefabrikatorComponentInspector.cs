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
                if (target is LineComponent line)
                {
                    line.ResetSharedData();
                    PrefabrikatorTool.Open(line);
                }
            }
            base.OnInspectorGUI();

        }
    }
}