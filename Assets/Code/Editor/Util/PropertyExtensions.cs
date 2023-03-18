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

        public static Vector3Property Create(string label, Shared<Vector3> watchedValue, Queue<ICommand> commandQueue)
        {
            void OnValueSet(Vector3 current, Vector3 previous)
            {
                commandQueue.Enqueue(new GenericCommand<Vector3>(watchedValue, previous, current));
            };
            return new Vector3Property(label, watchedValue, OnValueSet);
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
        public ToggleProperty(GUIContent content, Shared<bool> startValue, OnValueSetDelegate onValueSet)
            : base(content, startValue, onValueSet)
        {
            _shouldShowLabel = false;
        }

        protected override bool ShowPropertyField()
        {
            return EditorGUILayout.Toggle(_guiContent, WorkingValue);
        }
    }

    public class BezierProperty : CustomProperty<ControlPoint>
    {
        private bool _unfold = false;

        public BezierProperty(string label, Shared<ControlPoint> startValue, OnValueSetDelegate onValueSet)
            : base(label, startValue, onValueSet)
        {
            //
        }

        // #DG: need to rethink this to work as collection
        protected override ControlPoint ShowPropertyField()
        {
            _unfold = EditorGUILayout.Foldout(_unfold, GUIContent.none);
            ControlPoint point = WorkingValue;
            if (_unfold)
            {
                EditorGUILayout.BeginVertical();
                {
                    point.Position = EditorGUILayout.Vector3Field("Position", point.Position, null);
                    point.Tangent = EditorGUILayout.Vector3Field("Tangent", point.Tangent, null);
                }
                EditorGUILayout.EndVertical();
            }

            return point;
        }

        public static BezierProperty Create(string label, Shared<ControlPoint> watchedValue, Queue<ICommand> commandQueue)
        {
            void OnValueSet(ControlPoint current, ControlPoint previous)
            {
                commandQueue.Enqueue(new GenericCommand<ControlPoint>(watchedValue, previous, current));
            };
            return new BezierProperty(label, watchedValue, OnValueSet);
        }
    }
}
