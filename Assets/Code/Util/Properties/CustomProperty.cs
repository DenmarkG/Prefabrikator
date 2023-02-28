using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Prefabrikator
{
    public abstract class CustomProperty<T> where T : struct
    {
        protected enum EditMode
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
        public delegate T ValidateInputDelegate(T current);
        private OnValueSetDelegate OnValueSet = null;
        private ValidateInputDelegate OnValidate = null;

        protected bool _shouldShowLabel = true;
        protected GUIContent _guiContent = null;

        public event System.Action OnEditModeEnter = null;
        public event System.Action OnEditModeExit = null;

        private class CustomButton
        {
            private GUIContent _content = null;
            private System.Action<T> _onClick = null;

            public CustomButton(GUIContent content, System.Action<T> onClick)
            {
                _content = content;
                _onClick = onClick;
            }

            public bool Show() 
            {
                return GUILayout.Button(_content, GUILayout.Width(Extensions.IndentSize));
            }

            public void Click(T value)
            {
                _onClick(value);
            }
        }
        private List<CustomButton> _customButtons = null;

        public CustomProperty(string label, T startValue, OnValueSetDelegate onValueSet, ValidateInputDelegate onValidate = null)
        {
            _setValue.Set(startValue);
            Init(label, _setValue, onValueSet, onValidate);
        }

        public CustomProperty(string label, Shared<T> startValue, OnValueSetDelegate onValueSet, ValidateInputDelegate onValidate = null)
        {
            _setValue = startValue;
            Init(label, _setValue, onValueSet, onValidate);
        }

        public CustomProperty(GUIContent content, Shared<T> startValue, OnValueSetDelegate onValueSet, ValidateInputDelegate onValidate = null)
            : this(content.text, startValue, onValueSet, onValidate)
        {
            _guiContent = content;
        }

        private void Init(string label, T startValue, OnValueSetDelegate onValueSet, ValidateInputDelegate onValidate)
        {
            _label = label;
            _workingValue = _setValue;
            OnValueSet = onValueSet;
            OnValidate = onValidate;
            _setValueCopy = startValue;

            _setValue.OnValueChanged += OnValueChanged;
        }

        public T Update()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            {
                if (_shouldShowLabel)
                {
                    EditorGUILayout.LabelField(_label, GUILayout.MaxWidth(Extensions.LabelWidth));
                }

                if (_editMode == EditMode.Enabled)
                {
                    _workingValue = ShowPropertyField();
                    ShowCustomButtons();

                    if (GUILayout.Button(Constants.XButton))
                    {
                        _workingValue = _setValueCopy;
                        _editMode = EditMode.Disabled;
                        OnEditModeExit?.Invoke();
                    }

                    if (GUILayout.Button(Constants.CheckMark))
                    {
                        if (OnValidate != null)
                        {
                            _workingValue = OnValidate(_workingValue);
                        }

                        OnValueSet(_workingValue, _setValueCopy);
                        _setValue.Set(_workingValue);
                        _editMode = EditMode.Disabled;
                        OnEditModeExit?.Invoke();
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        ShowPropertyField();
                        ShowCustomButtons();
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button(Constants.EditButton, GUILayout.MaxWidth(40)))
                    {
                        _editMode = EditMode.Enabled;
                        _setValueCopy = _setValue;
                        OnEditModeEnter?.Invoke();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            return _workingValue;
        }

        private void ShowCustomButtons()
        {
            if (_customButtons != null && _customButtons.Count > 0)
            {
                int numControls = _customButtons.Count;
                CustomButton button = null;
                for (int i = 0; i < numControls; ++i)
                {
                    button = _customButtons[i];
                    if (button.Show())
                    {
                        button.Click(_workingValue);
                    }
                }
            }
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

        public void AddCustomButton(GUIContent content, System.Action<T> onClick)
        {
            if (_customButtons == null)
            {
                _customButtons = new List<CustomButton>();
            }

            _customButtons.Add(new CustomButton(content, onClick));
        }

        protected abstract T ShowPropertyField();
    }
}