using UnityEngine;
using UnityEditor;
using Prefabrikator.Runtime;
using System.Collections.Generic;

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

        public abstract TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies);

        // #DG: TODO: Modifiers are removed when saving
        public abstract void OnRemoved(IRuntimeShape target, Transform[] proxies);
        public abstract void Teardown(IRuntimeShape target, Transform[] proxies);
    }
}