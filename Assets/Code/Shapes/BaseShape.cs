using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    [System.Serializable]
    public abstract class BaseShape
    {
        public abstract int Count { get; }
        public abstract List<Transform> Collection { get; }
        public abstract IShapeData GetShapeData();
        public abstract void SetShapeData(IShapeData shapeData);
        public abstract Vector3 GetDefaultPositionAtIndex(int index);
        public abstract void AddTransform(Transform xform);
        public abstract void Refresh();

        public abstract IShapeData ShapeData { get; }
    }
}