using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPoint
{
    public Vector3 Position = new Vector3();
    public Vector3 Tangent = Vector3.up;

    public BezierPoint(Vector3 position)
    {
        Position = position;
        Tangent += position;
    }

    public BezierPoint(Vector3 position, Vector3 tangent)
    {
        Position = position;
        Tangent = position + tangent;
    }

    public override bool Equals(object obj)
    {
        return this.Equals(obj as BezierPoint);
    }

    public bool Equals(BezierPoint point)
    {
        if (Object.ReferenceEquals(point, null))
        {
            return false;
        }

        if (Object.ReferenceEquals(this, point))
        {
            return true;
        }

        if (this.GetType() != point.GetType())
        {
            return false;
        }

        return (Position == point.Position) && (Tangent == point.Tangent);
    }

    public override int GetHashCode()
    {
        int hashCode = -881549476;
        hashCode = hashCode * -1521134295 + Position.GetHashCode();
        hashCode = hashCode * -1521134295 + Tangent.GetHashCode();
        return hashCode;
    }
}
