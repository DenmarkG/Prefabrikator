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

        protected string Label => _label;
        private string _label = string.Empty;

        protected Shared<T> SetValue => _setValue;
        private Shared<T> _setValue = new Shared<T>();
        private T _setValueCopy = default(T);

        protected T WorkingValue
        {
            get { return _workingValue; }
            set { _workingValue = value; }
        }
        private T _workingValue = default(T);

        private EditMode _editMode = EditMode.Disabled;

        public delegate void OnValueSetDelegate(T current, T previous);
        private OnValueSetDelegate OnValueSet = null;

        protected bool _shouldShowLabel = true;

        public CustomProperty(string label, T startValue, OnValueSetDelegate onValueSet)
        {
            _setValue.Set(startValue);
            Init(label, _setValue, onValueSet);
        }

        public CustomProperty(string label, Shared<T> startValue, OnValueSetDelegate onValueSet)
        {
            _setValue = startValue;
            Init(label, _setValue, onValueSet);
        }

        private void Init(string label, T startValue, OnValueSetDelegate onValueSet)
        {
            _label = label;
            _workingValue = _setValue;
            OnValueSet = onValueSet;
            _setValueCopy = startValue;

            _setValue.OnValueChanged += OnValueChanged;
        }

        public T Update()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(PrefabrikatorTool.MaxWidth));
            {
                if (_shouldShowLabel)
                {
                    EditorGUILayout.LabelField(_label, GUILayout.Width(Extensions.LabelWidth));
                }

                if (_editMode == EditMode.Enabled)
                {
                    _workingValue = ShowPropertyField();
                    if (GUILayout.Button("O"))
                    {
                        OnValueSet(_workingValue, _setValueCopy);
                        _setValue.Set(_workingValue);
                        _editMode = EditMode.Disabled;
                    }
                    if (GUILayout.Button("X"))
                    {
                        _workingValue = _setValueCopy;
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
                        _setValueCopy = _setValue;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            return _workingValue;
        }

        public void SetDefaultValue(T defaultValue)
        {
            _setValue.Set(defaultValue);
            _workingValue = _setValue;
        }

        private void OnValueChanged(T newSetValue)
        {
            if (newSetValue.Equals(_workingValue) == false)
            {
                _workingValue = newSetValue;
            }
        }

        protected abstract T ShowPropertyField();
    }
}