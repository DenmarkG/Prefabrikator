using UnityEditor;
using UnityEngine;

namespace Prefabrikator
{
    using static Constants;

    [CustomPropertyDrawer(typeof(Shared<int>))]
    public class SharedIntPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                SerializedProperty valueProperty = property.FindPropertyRelative("_value");
                EditorGUI.PropertyField(position, valueProperty, new GUIContent(property.displayName));
            }
            EditorGUI.EndProperty();
        }
    }
}