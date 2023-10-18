using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public struct TransformProxy
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public TransformProxy(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public TransformProxy(Transform other)
        {
            Position = other.position;
            Rotation = other.rotation;
            Scale = other.localScale;
        }

        public static implicit operator TransformProxy(Transform other) => new TransformProxy(other);
    }
}