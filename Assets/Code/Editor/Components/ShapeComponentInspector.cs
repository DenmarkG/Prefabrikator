using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using System.Threading;
using System.Collections;
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


            var lineProperty = serializedObject.FindProperty("_line");
            var collection = serializedObject.FindProperty("_collection");
            var modifiers = serializedObject.FindProperty("_modifiers");

            main.Add(new PropertyField(lineProperty));
            main.Add(new PropertyField(collection));

            var list = new ListView();
            list.showAddRemoveFooter = false;
            list.BindProperty(modifiers);

            main.Add(list);


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