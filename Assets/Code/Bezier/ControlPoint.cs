using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ControlPoint : IEquatable<ControlPoint>
{
    public static readonly ControlPoint Default = new ControlPoint()
    {
        Position = new Vector3(),
        Tangent = Vector3.up
    };

    public Vector3 Position;
    public Vector3 Tangent;

    public ControlPoint(Vector3 position)
    {
        Position = position;
        Tangent = Vector3.up;
    }

    public ControlPoint(Vector3 position, Vector3 tangent)
    {
        Position = position;
        Tangent = position + tangent;
    }

    public bool Equals(ControlPoint other)
    {
        return (Position == other.Position) && (Tangent == other.Tangent);
    }

    public override int GetHashCode()
    {
        int hashCode = -881549476;
        hashCode = hashCode * -1521134295 + Position.GetHashCode();
        hashCode = hashCode * -1521134295 + Tangent.GetHashCode();
        return hashCode;
    }
}
