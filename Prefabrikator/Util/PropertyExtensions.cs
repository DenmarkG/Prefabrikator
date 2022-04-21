using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class PropertyExtensions
    {
        public class Vector3Property
        {
            private string _label = string.Empty;
            private Vector3 _startValue = Vector3.zero;
            private Vector3 _currentValue = Vector3.zero;

            private bool _isValueChanging = false;

            public Vector3Property(string label, Vector3 startValue)
            {
                _label = label;
                _startValue = startValue;
                _currentValue = _startValue;
            }

            public Vector3 Update()
            {
                EditorGUILayout.LabelField(_label, GUILayout.Width(ArrayToolExtensions.LabelWidth));
                Vector3 tempValue = EditorGUILayout.Vector3Field(string.Empty, _currentValue, null);
                if (_currentValue != tempValue)
                {
                    if (!_isValueChanging)
                    {
                        _isValueChanging = true;
                        Debug.Log("Value change started");
                    }

                    Debug.Log($"Value updating from {_currentValue} to {tempValue}");
                    _currentValue = tempValue;
                }
                else
                {
                    if (_isValueChanging)
                    {
                        _isValueChanging = false;
                        _startValue = _currentValue;
                        _currentValue = tempValue;
                        Debug.Log("Value change ended");
                    }
                }

                return _startValue;
            }
        }
    }
}
