using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShape
    {
        int Count { get; }
        List<Transform> Collection { get; }
        
        Vector3 GetDefaultPositionAtIndex(int index);
        void AddTransform(Transform xform);
        void Refresh();
    }
}