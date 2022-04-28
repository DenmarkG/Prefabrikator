using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{

    public abstract class CustomProperty<T>
    {
        public enum EditMode
        {
            Enabled,
            Disabled
        }

        private string _label = string.Empty;

        protected T SetValue => _setValue;
        private T _setValue = default(T);

        protected T WorkingValue
        {
            get { return _workingValue; }
            set { _workingValue = value; }
        }
        private T _workingValue = default(T);

        private EditMode _editMode = EditMode.Disabled;

        public delegate void OnValueSetDelegate(T current, T previous);
        private OnValueSetDelegate OnValueSet = null;

        public CustomProperty(string label, T startValue, OnValueSetDelegate onValueSet)
        {
            _label = label;
            _setValue = startValue;
            _workingValue = _setValue;
            OnValueSet = onValueSet;
        }

        public T Update()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(PrefabrikatorTool.MaxWidth));
            {
                EditorGUILayout.LabelField(_label, GUILayout.Width(Extensions.LabelWidth));
                if (_editMode == EditMode.Enabled)
                {
                    _workingValue = ShowPropertyField();
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
                        ShowPropertyField();
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

        public void SetDefaultValue(T defaultValue)
        {
            _setValue = defaultValue;
            _workingValue = _setValue;
        }

        protected abstract T ShowPropertyField();
    }
}