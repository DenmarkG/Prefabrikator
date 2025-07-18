using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator
{
    [CustomEditor(typeof(ShapeComponent), editorForChildClasses: true)]
    public class ShapeComponentInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement main = new();

            main.Add(new Button(OnClick) { text = "Edit" });

            var iterator = serializedObject.GetIterator();

            bool isFirst = true;
            while (iterator.NextVisible(isFirst))
            {
                //if (SerializedProperty.EqualContents(iterator, property))
                //    continue; // Skip the root itself

                var field = new PropertyField(iterator);
                field.BindProperty(iterator);
                main.Add(field);

                isFirst = false;
            }

            return main;
        }

        private void OnClick()
        {
            if (target is LineComponent line)
            {
                line.ResetSharedData();
                PrefabrikatorTool.Open(line);
            }
        }
    }
}