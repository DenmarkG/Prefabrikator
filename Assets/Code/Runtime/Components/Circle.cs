using Prefabrikator.Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [System.Serializable]
    public class Circle : IShape
    {
        public float Radius;
        public Vector3 Center;

        public void Deconstruct(out float radius, out Vector3 center)
        {
            radius = Radius;
            center = Center;
        }
    }

    public static class CirlceExtensions
    {
        public static Vector3 GetDefaultPositionAtIndex(int index, int count, float radius, Vector3 center)
        {
            if (count <= 0)
                return center;

            const float degrees = Mathf.PI * 2;
            float angle = (degrees / count);

            float t = angle * index;
            float x = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;

            // #DG: TODO: Account for additional rotation
            return new Vector3(x, 0f, z) + center;
        }

        public static Vector3 GetDefaultPositionAtIndex(this Circle circle, int index, int count)
        {
            return GetDefaultPositionAtIndex(index, count, circle.Radius, circle.Center);
        }


        // #DG: TODO: replicate this for proxies
        public static void Refresh(this Circle circle, List<Transform> transforms)
        {
            if (transforms == null)
                return;

            int count = transforms.Count;
            Transform current = null;
            for (int i = 0; i < count; ++i)
            {
                if ((current = transforms[i]) != null)
                {
                    current.position = GetDefaultPositionAtIndex(circle, i, count);
                }
            }
        }
    }
}