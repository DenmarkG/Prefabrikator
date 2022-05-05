using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class Vector3Property : CustomProperty<Vector3>
    {
        public Vector3Property(string label, Shared<Vector3> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override Vector3 ShowPropertyField()
        {
            return EditorGUILayout.Vector3Field(Label, WorkingValue, null);
        }
    }

    public class QuaternionProperty : CustomProperty<Quaternion>
    {
        public QuaternionProperty(string label, Quaternion startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            //
        }

        protected override Quaternion ShowPropertyField()
        {
            Vector3 localEulerRotation = WorkingValue.eulerAngles;
            localEulerRotation = EditorGUILayout.Vector3Field(string.Empty, localEulerRotation);
            
            return Quaternion.Euler(localEulerRotation);
        }
    }

    public class CountProperty : CustomProperty<int>
    {
        public CountProperty(string label, int startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            //
        }

        protected override int ShowPropertyField()
        {
            EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
            {
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    if (WorkingValue > 0)
                    {
                        --WorkingValue;
                    }
                }

                float spacing = 30;
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(WorkingValue.ToString(), GUILayout.Width(spacing));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    if (WorkingValue < int.MaxValue - 1)
                    {
                        ++WorkingValue;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            return WorkingValue;
        }
    }

    public class FloatProperty : CustomProperty<float>
    {
        public FloatProperty(string label, Shared<float> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override float ShowPropertyField()
        {
            return EditorGUILayout.FloatField(Label, WorkingValue);
        }
    }
}
