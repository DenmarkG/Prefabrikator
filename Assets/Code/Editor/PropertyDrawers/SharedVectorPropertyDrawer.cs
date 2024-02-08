using UnityEditor;
using UnityEngine;

namespace Prefabrikator
{
    using static Constants;

    [CustomPropertyDrawer(typeof(Shared<Vector3>))]
    public class SharedVectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                EditorGUIUtility.labelWidth = ThreeQuarterLabelWidth;
                SerializedProperty valueProperty = property.FindPropertyRelative("_value");
                EditorGUI.PropertyField(position, valueProperty, new GUIContent(property.displayName));
            }
            EditorGUI.EndProperty();
        }
    }
}