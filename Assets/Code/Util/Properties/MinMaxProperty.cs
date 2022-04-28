using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class MinMaxProperty : CustomProperty<MinMax>
    {
        public MinMaxProperty(string label, MinMax startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            //
        }

        protected override MinMax ShowPropertyField()
        {
            MinMax temp = new MinMax(WorkingValue);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUIUtility.labelWidth = 50;
                temp.Min = EditorGUILayout.FloatField("Min", temp.Min, GUILayout.Width(150));
                temp.Max = EditorGUILayout.FloatField("Max", temp.Max, GUILayout.Width(150));
            }
            EditorGUILayout.EndHorizontal();

            return temp;
        }
    }
}