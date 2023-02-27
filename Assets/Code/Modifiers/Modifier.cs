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
        public static readonly string PositionNoise = "Position Noise";
        public static readonly string DropToFloor = "Drop to Floor";
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
            EditorGUILayout.BeginVertical(new GUIStyle("Tooltip"), GUILayout.MaxWidth(PrefabrikatorTool.MaxWidth - Extensions.IndentSize), GUILayout.ExpandWidth(false));
            {
                OnInspectorUpdate();
            }
            EditorGUILayout.EndVertical();
        }

        protected abstract void OnInspectorUpdate();
        public abstract void Process(GameObject[] objs);

        public abstract void OnRemoved();
    }
}