using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class ModifierType
    {
        public static readonly string ScaleRandom = "Scale Random";
        public static readonly string ScaleUniform = "Scale Uniform";
        public static readonly string RotationRandom = "Rotation Random";
        public static readonly string RotationUniform = "Rotation Uniform";
        public static readonly string FollowCurve = "Follow Curve";
        public static readonly string IncrementalRotation = "Incremental Rotation";
        public static readonly string IncrementalScale = "Incremental Scale";
    }

    public abstract class Modifier
    {
        protected ArrayCreator Owner => _owner;
        private ArrayCreator _owner = null;

        protected abstract string DisplayName { get; }

        private bool _isExpanded = true;

        public Modifier(ArrayCreator owner)
        {
            _owner = owner;
        }

        public void UpdateInspector()
        {
            _isExpanded = EditorGUILayout.Foldout(_isExpanded, DisplayName);
            if (_isExpanded)
            {
                EditorGUI.indentLevel++;
                if (GUILayout.Button("-"))
                {
                    _owner.CommandQueue.Enqueue(new ModifierRemoveCommand(this, _owner));
                }
                
                OnInspectorUpdate();
                EditorGUI.indentLevel--;
            }            
        }

        protected abstract void OnInspectorUpdate();
        public abstract void Process(GameObject[] objs);

        public abstract void OnRemoved();
    }
}