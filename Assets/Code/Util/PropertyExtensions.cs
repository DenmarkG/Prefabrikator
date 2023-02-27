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

    public class LayerProperty : CustomProperty<LayerMask>
    {
        public LayerProperty(string label, Shared<LayerMask> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            //
        }

        protected override LayerMask ShowPropertyField()
        {
            return EditorGUILayout.LayerField(Label, WorkingValue);
        }
    }

    //public class BoolProperty : CustomProperty<bool>
    //{
    //    public BoolProperty(string label, Shared<bool> startValue, OnValueSetDelegate onValueSet)
    //        : base(label, startValue, onValueSet)
    //    {
    //        //
    //    }

    //    protected override bool ShowPropertyField()
    //    {
    //        return EditorGUILayout.ToggleLeft(Label, WorkingValue);
    //    }
    //}
}
