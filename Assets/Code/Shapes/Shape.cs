using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShape
    {
        int MaxCount { get; }
        int MinCount { get; }
        List<Transform> Collection { get; }
        List<Modifier> Modidfiers { get; }
        
        Vector3 GetDefaultPositionAtIndex(int index);
        void AddTransform(Transform xform);
        void Refresh();

        //void AddCloneFromIndex(int index);
    }
}