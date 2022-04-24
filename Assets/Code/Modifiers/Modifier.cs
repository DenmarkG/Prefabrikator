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

        public Modifier(ArrayCreator owner)
        {
            _owner = owner;
        }

        public void UpdateInspector()
        {
            EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
            {
                GUILayout.Label(DisplayName);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-"))
                {
                    _owner.RemoveModifier(this);
                }
            }
            EditorGUILayout.EndHorizontal();

            OnInspectorUpdate();
        }

        protected abstract void OnInspectorUpdate();
        public abstract void Process(GameObject[] objs);
    }
}