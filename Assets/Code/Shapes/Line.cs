using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public struct LineData : IShapeData
    {
        public Vector3 Start;
        public Vector3 Offset;

        public readonly void Deconstruct(out Vector3 start, out Vector3 offset)
        {
            start = Start;
            offset = Offset;
        }
    }

    public class Line : Shape<LineData>
    {
        public static readonly int MaxCount = 50;
        public static readonly int MinCount = 0;

        public static Vector3 GetPositionAtIndex(int i, Vector3 start, Vector3 offset)
        {
            return start + (offset * i);
        }

        public override Vector3 GetDefaultPositionAtIndex(int index, LineData data)
        {
            var (start, offset) = data;
            return start + (offset * index);
        }

        public static bool IsValid(List<Transform> transforms)
        {
            foreach (Transform obj in transforms)
            {
                if (obj == null)
                {
                    return false;
                }
            }

            return true;
        }
            
        // Validate the line before calling this. 
        public static void Refresh(List<Transform> transforms, Vector3 start, Vector3 offset)
        {
            int numObjects = transforms.Count;
            Transform current = null;

            Undo.RecordObjects(transforms.ToArray(), "Array Update");
            for (int i = 0; i < numObjects; ++i)
            {
                if ((current = transforms[i]) != null)
                {
                    current.position = GetPositionAtIndex(i, start, offset);
                }
            }
        }
    }
}