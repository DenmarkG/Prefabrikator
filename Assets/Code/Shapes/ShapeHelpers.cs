using Prefabrikator.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public static class ShapeHelpers
    {
        public static readonly int DefaultMaxCount;

        public static void AddTranform(this BaseShape shape, Transform xform)
        {
            if (xform == null)
                return;

            shape.Collection.Add(xform);
        }

        public static void AddRange(this BaseShape shape, IEnumerable<Transform> xforms)
        {
            if (xforms == null)
                return;

            shape.Collection.AddRange(xforms);
        }

        // #DG: add option to delete? 
        public static void SetTransforms(this BaseShape shape, IEnumerable<Transform> xforms)
        {
            shape.Collection.Clear();

            if (xforms == null)
                return;

            shape.Collection.AddRange(xforms);
        }

        public static void RemoveTransformAtIndex(this BaseShape shape, int index)
        {
            if (index < 0 || index > shape.Count)
                return;

            shape.Collection.RemoveAt(index);
        }

        public static void ApplyToAll(this BaseShape shape, ApplicatorDelegate applicator)
        {
            int numObjs = shape.Collection.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(shape.Collection[i]);
            }
        }

        public static void ApplyToAll(this BaseShape shape, IndexedApplicatorDelegate applicator)
        {
            int numObjs = shape.Collection.Count;
            for (int i = 0; i < numObjs; ++i)
            {
                applicator(shape.Collection[i], i);
            }
        }

        public static void SetTransformFromProxy(this GameObject obj, TransformProxy proxy)
        {
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(proxy.Position, proxy.Rotation);
                obj.transform.localScale = proxy.Scale;
            }
        }

        public static IShapeData CreateDefaultData(ShapeType shapeType)
        {
            switch (shapeType)
            {
                case ShapeType.Line:
                    return LineData.Default;
                case ShapeType.Grid:
                    break;
                case ShapeType.Circle:
                    return CircleData.Default;
                case ShapeType.Arc:
                    break;
                case ShapeType.Sphere:
                    break;
                case ShapeType.ScatterSphere:
                    break;
                case ShapeType.ScatterBox:
                    break;
                case ShapeType.ScatterPlane:
                    break;
                case ShapeType.Ellipse:
                    break;
                default:
                    break;
            }

            return null;
        }
    }
}