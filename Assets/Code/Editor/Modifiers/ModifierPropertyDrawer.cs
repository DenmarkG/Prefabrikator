using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    [CustomPropertyDrawer(typeof(Modifier), true)]
    public class ModifierDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a root container
            var root = new VisualElement();

            // Display the type name as a label
            var typeLabel = new Label(property.managedReferenceFullTypename.Split('.').Last());
            typeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(typeLabel);

            // Iterate through all visible child properties
            var iterator = property.Copy();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(iterator, property))
                    continue; // Skip the root itself

                var field = new PropertyField(iterator);
                field.BindProperty(iterator);
                root.Add(field);

                enterChildren = false;
            }

            return root;
        }
    }
}