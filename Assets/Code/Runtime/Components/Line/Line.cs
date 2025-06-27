using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{

    [System.Serializable]
    public struct Line : IShape
    {
        public Vector3 Start;
        public Vector3 Offset;

        public void Deconstruct(out Vector3 start, out Vector3 offset)
        {
            start = Start;
            offset = Offset;
        }
    }

    public static class LineExtensions
    {
        public static Vector3 GetPositionAtIndex(int i, Vector3 start, Vector3 offset)
        {
            return start + (offset * i);
        }

        public static Vector3 GetPositionAtIndex(this Line line, int i)
        {
            return line.Start + (line.Offset * i);
        }

        public static void Refresh(this Line line, List<Transform> transforms)
        {
            if (transforms == null)
                return;

            int numObjects = transforms.Count;

            Transform current = null;

            for (int i = 0; i < numObjects; ++i)
            {
                if ((current = transforms[i]) != null)
                {
                    current.position = GetPositionAtIndex(i, line.Start, line.Offset);
                }
            }
        }
    }
}