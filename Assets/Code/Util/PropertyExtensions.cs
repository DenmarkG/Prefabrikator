using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class Vector3Property : CustomProperty<Vector3>
    {
        public Vector3Property(string label, Shared<Vector3> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = true;
        }

        protected override Vector3 ShowPropertyField()
        {
            return EditorGUILayout.Vector3Field(string.Empty, WorkingValue, null);
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
                if (GUILayout.Button("-", GUILayout.Width(10)))
                {
                    if (WorkingValue > 0)
                    {
                        --WorkingValue;
                    }
                }

                EditorGUILayout.LabelField(WorkingValue.ToString());

                if (GUILayout.Button("+", GUILayout.Width(10)))
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
