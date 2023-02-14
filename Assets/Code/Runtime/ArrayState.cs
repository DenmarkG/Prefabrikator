using UnityEngine;
using System.Collections.Generic;

namespace Prefabrikator
{
    // #DG: Give this an Abstract factory

    [System.Serializable]
    public abstract class ArrayState
    {
        public ShapeType Type { get; }
        public int Count = 0;
        public GameObject[] CreatedObjects;

        public ArrayState(ShapeType type)
        {
            Type = type;
        }
    }
}
