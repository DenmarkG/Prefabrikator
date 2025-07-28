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
        public static readonly string RadialNoise = "Radial Noise";
        public static readonly string IncrementalRotation = "Incremental Rotation";
        public static readonly string IncrementalScale = "Incremental Scale";
        public static readonly string PositionNoise = "Position Noise";
        public static readonly string DropToFloor = "Drop to Floor";
    }


    //public class ModifierCollection
    //{
    //    public 
    //}

    [System.Serializable]
    public abstract class Modifier
    {
        public abstract string DisplayName { get; }

        public void UpdateInspector(IShape target)
        {
            EditorGUILayout.BeginVertical(new GUIStyle("Tooltip"), GUILayout.MaxWidth(Constants.MaxWidth - Constants.IndentSize), GUILayout.ExpandWidth(false));
            {
                OnInspectorUpdate(target);
            }
            EditorGUILayout.EndVertical();
        }

        protected abstract void OnInspectorUpdate(IShape target);
        public abstract TransformProxy[] Process(IShape target, TransformProxy[] proxies);

        // #DG: Modifiers are removed when saving
        public abstract void OnRemoved(IShape target);
        public abstract void Teardown(IShape target);

        public OnValueSetDelegate<T> CreateCommand<T>(Shared<T> field, IShape target) where T : struct
        {
            return (T current, T previous) => { target.CommandQueue.Enqueue(new GenericCommand<T>(field, previous, current)); };
        }
    }
}