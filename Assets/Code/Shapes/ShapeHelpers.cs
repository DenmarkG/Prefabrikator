using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public static class ShapeHelpers
    {
        public static void AddTranform(this IShape shape, Transform xform)
        {
            if (xform == null)
                return;

            shape.Collection.Add(xform);
        }

        public static void AddRange(this IShape shape, IEnumerable<Transform> xforms)
        {
            if (xforms == null)
                return;

            shape.Collection.AddRange(xforms);
        }

        // #DG: add option to delete? 
        public static void SetTransforms(this IShape shape, IEnumerable<Transform> xforms)
        {
            shape.Collection.Clear();

            if (xforms == null)
                return;

            shape.Collection.AddRange(xforms);
        }

        public static void ApplyToAll(this IShape shape, ApplicatorDelegate applicator)
        {
            int numObjs = shape.Collection.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(shape.Collection[i]);
            }
        }

        public static void ApplyToAll(this IShape shape, IndexedApplicatorDelegate applicator)
        {
            int numObjs = shape.Collection.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(shape.Collection[i], i);
            }
        }
    }
}