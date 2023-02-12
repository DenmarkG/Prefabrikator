using UnityEngine;

namespace Prefabrikator
{
    // #DG: Give this an Abstract factory

    [System.Serializable]
    public abstract class ArrayData
    {
        public ShapeType Type;
        public int Count = 0;
        public GameObject Prefab = null;

        public ArrayData(ShapeType type, GameObject prefab, Quaternion targetRotation)
        {
            Type = type;
            Prefab = prefab;
        }
    }
}
