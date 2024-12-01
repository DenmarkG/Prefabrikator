using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShapeData { }

    public abstract class Shape<T> where T : IShapeData 
    {
        public abstract int MaxCount { get; }
        public abstract int MinCount { get; }

        public abstract T ShapeData { get; }

        public abstract Vector3 GetDefaultPositionAtIndex(int index, T data);

        public List<Transform> Collection { get; protected set; } = new List<Transform>();

        public void AddTranform(Transform xform)
        {
            if (xform == null)
                return;

            Collection.Add(xform);
        }

        public void AddRange(IEnumerable<Transform> xforms)
        {
            if (xforms == null)
                return;

            Collection.AddRange(xforms);
        }

        public virtual void SetTransforms(IEnumerable<Transform> xforms)
        {
            Collection.Clear();

            if (xforms == null)
                return;

            Collection.AddRange(xforms);
        }

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