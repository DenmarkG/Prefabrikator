using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShape
    {
        int Count { get; }
        List<Transform> Collection { get; }
        IShapeData GetShapeData();
        void SetShapeData(IShapeData shapeData);
        Vector3 GetDefaultPositionAtIndex(int index);
        void AddTransform(Transform xform);
        void Refresh();
    }
}