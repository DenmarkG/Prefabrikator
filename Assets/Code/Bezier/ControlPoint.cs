using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    [System.Serializable]
    public struct ControlPoint : IEquatable<ControlPoint>
    {
        public static readonly ControlPoint Default = new ControlPoint()
        {
            Position = new Shared<Vector3>(new Vector3()),
            Tangent = new Shared<Vector3>(Vector3.up)
        };

        public Shared<Vector3> Position;
        public Shared<Vector3> Tangent;

        public ControlPoint(Vector3 position)
        {
            Position = new Shared<Vector3>(position);
            Tangent = new Shared<Vector3>(Position + Vector3.up);
        }

        public ControlPoint(Vector3 position, Vector3 tangent)
        {
            Position = new Shared<Vector3>(position);
            Tangent = new Shared<Vector3>(tangent);
        }

        public bool Equals(ControlPoint other)
        {
            return (Position == other.Position) && (Tangent == other.Tangent);
        }
    }
}