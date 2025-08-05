using System;
using System.Numerics;

namespace Prefabrikator.Runtime
{
    public interface IRuntimeShapeData { }

    public interface IRuntimeShape
    {
        ShapeType BaseShapeType { get; }
    }
}
