using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class PropertyExtensions
    {
        public enum EditMode
        {
            Enabled,
            Disabled
        }

        public class Vector3Property
        {
            private string _label = string.Empty;
            private Vector3 _setValue = Vector3.zero;
            private Vector3 _workingValue = Vector3.zero;

            private EditMode _editMode = EditMode.Disabled;

            public delegate void OnValueSetDelegate(Vector3 current, Vector3 previous);
            private OnValueSetDelegate OnValueSet = null;

            public Vector3Property(string label, Vector3 startValue, OnValueSetDelegate onValueSet)
            {
                _label = label;
                _setValue = startValue;
                _workingValue = _setValue;
                OnValueSet = onValueSet;
            }

            public void SetDefaultValue(Vector3 defaultValue)
            {
                _setValue = defaultValue;
                _workingValue = _setValue;
            }

            public Vector3 Update()
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(_label, GUILayout.Width(Extensions.LabelWidth));
                    if (_editMode == EditMode.Enabled)
                    {
                        _workingValue = EditorGUILayout.Vector3Field(string.Empty, _workingValue, null);
                        if (GUILayout.Button("O"))
                        {
                            OnValueSet(_workingValue, _setValue);
                            _setValue = _workingValue;
                            _editMode = EditMode.Disabled;
                        }
                        if (GUILayout.Button("X"))
                        {
                            _workingValue = _setValue;
                            _editMode = EditMode.Disabled;
                        }
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.Vector3Field(string.Empty, _setValue, null);
                        }
                        EditorGUI.EndDisabledGroup();
                        if (GUILayout.Button("..."))
                        {
                            _editMode = EditMode.Enabled;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                return _workingValue;
            }
        }
    }
}
