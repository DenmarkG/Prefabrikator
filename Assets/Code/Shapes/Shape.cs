using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShapeData { }

    public abstract class Shape<T> where T : IShapeData 
    {
        public abstract int MaxCount { get; }
        public abstract int MinCount { get; }
        public abstract Vector3 GetDefaultPositionAtIndex(int index, T data);

        public List<GameObject> Collection { get; }

        public void ApplyToAll(ApplicatorDelegate applicator)
        {
            int numObjs = Collection.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(Collection[i]);
            }
        }

        public void ApplyToAll(IndexedApplicatorDelegate applicator)
        {
            int numObjs = Collection.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(Collection[i], i);
            }
        }
    }
}