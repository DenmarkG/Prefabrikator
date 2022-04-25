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
                float width = PrefabrikatorTool.MaxWidth / 3;
                temp.Min = EditorGUILayout.FloatField("Min", temp.Min, GUILayout.MaxWidth(width));
                temp.Max = EditorGUILayout.FloatField("Max", temp.Max/*, GUILayout.MaxWidth(75)*/);
            }
            EditorGUILayout.EndHorizontal();

            return temp;
        }
    }
}