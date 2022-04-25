using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class PropertyExtensions
    {


        // #DG: move this to it's own file
        public class Vector3Property : CustomProperty<Vector3>
        {
            public Vector3Property(string label, Vector3 startValue, OnValueSetDelegate onValueSet)
                : base(label, startValue, onValueSet)
            {
                //
            }

            protected override Vector3 ShowPropertyField()
            {
                return EditorGUILayout.Vector3Field(string.Empty, WorkingValue, null);
            }
        }
    }
}
