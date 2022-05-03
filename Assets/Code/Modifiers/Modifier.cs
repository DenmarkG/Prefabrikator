using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public enum ModifierType
    {
        ScaleRandom,
        ScaleUniform
    }

    public abstract class Modifier
    {
        protected ArrayCreator Owner => _owner;
        private ArrayCreator _owner = null;

        protected abstract string DisplayName { get; }

        private bool _isExpanded = false;

        public Modifier(ArrayCreator owner)
        {
            _owner = owner;
        }

        public void UpdateInspector()
        {
            _isExpanded = EditorGUILayout.Foldout(_isExpanded, DisplayName);
            if (_isExpanded)
            {
                if (GUILayout.Button("-"))
                {
                    _owner.CommandQueue.Enqueue(new ModifierRemoveCommand(this, _owner));
                }

                EditorGUI.indentLevel++;
                OnInspectorUpdate();
                EditorGUI.indentLevel--;
            }            
        }

        protected abstract void OnInspectorUpdate();
        public abstract void Process(GameObject[] objs);

        public abstract void OnRemoved();
    }
}