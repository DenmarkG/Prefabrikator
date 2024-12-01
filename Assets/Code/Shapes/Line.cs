using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Shapes
{
    [System.Serializable]
    public struct LineData : IShapeData
    {
        public static readonly LineData Default = new LineData() { Offset = DefaultOffset };
        public static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public Vector3 Start;
        public Vector3 Offset;

        public LineData(Vector3 start)
        {
            Start = start;
            Offset = DefaultOffset;
        }

        public LineData(Vector3 start, Vector3 offset)
        {
            Start = start;
            Offset = offset;
        }

        public LineData(LineData other)
            : this(other.Start, other.Offset)
        {
        }

        public readonly void Deconstruct(out Vector3 start, out Vector3 offset)
        {
            start = Start;
            offset = Offset;
        }
    }

    [System.Serializable]
    public class Line : Shape<LineData>
    {
        public override int MaxCount => 50;
        public override int MinCount => 0;

        public override LineData ShapeData => _lineData;
        [SerializeField] private LineData _lineData = LineData.Default;

        public Line()
        {
            _lineData = LineData.Default;
        }

        public Line(Vector3 start)
        {
            _lineData = new LineData(start);
        }

        public Line(Vector3 start, Vector3 offset)
        {
            _lineData = new LineData(start, offset);
        }

        public Line(LineData lineData)
            : this(lineData.Start, lineData.Offset) { }

        public void SetOffset(Vector3 offset)
        {
            _lineData.Offset = offset;
        }

        public void SetStart(Vector3 start)
        {
            _lineData.Start = start;
        }

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

        public void Refresh()
        {
            Refresh(Collection, _lineData.Start, _lineData.Offset);
        }
    }
}