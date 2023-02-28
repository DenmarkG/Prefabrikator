using System.Collections.Generic;
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

    public class FloatSlider : CustomProperty<float>
    {
        public float Max = .999f;
        public float Min = .01f;

        public FloatSlider(string label, Shared<float> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override float ShowPropertyField()
        {
            return EditorGUILayout.Slider(WorkingValue, Min, Max, null);
        }
    }

    public class IntProperty : CustomProperty<int>
    {
        public IntProperty(string label, Shared<int> startValue, OnValueSetDelegate onValueSet, ValidateInputDelegate onValidate)
            : base(label, startValue, onValueSet, onValidate)
        {
            _shouldShowLabel = false;
        }

        protected override int ShowPropertyField()
        {
            return EditorGUILayout.IntField(Label, WorkingValue);
        }
    }

    public class LayerMaskProperty : CustomProperty<LayerMask>
    {
        public LayerMaskProperty(string label, Shared<LayerMask> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override LayerMask ShowPropertyField()
        {
            string[] options = BuildLayerList();

            return EditorGUILayout.MaskField(Label, WorkingValue, options);
        }

        private string[] BuildLayerList()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < 32; ++i)
            {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                {
                    names.Add(name);
                }
            }

            return names.ToArray();
        }
    }

    public class BoolProperty : CustomProperty<bool>
    {
        public BoolProperty(string label, Shared<bool> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override bool ShowPropertyField()
        {
            return EditorGUILayout.ToggleLeft(Label, WorkingValue);
        }
    }

    public class ToggleProperty : CustomProperty<bool>
    {
        public ToggleProperty(string label, Shared<bool> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override bool ShowPropertyField()
        {
            return EditorGUILayout.Toggle(Label, WorkingValue);
        }
    }
}
