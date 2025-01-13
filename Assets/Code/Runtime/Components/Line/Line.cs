using Codice.Client.BaseCommands;
using JetBrains.Annotations;
using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [System.Serializable]
    public struct LineData : IShapeData
    {
        public Shared<Vector3> Start => _start;
        [SerializeField] private Shared<Vector3> _start; 
        
        public Shared<Vector3> Offset => _offset;
        [SerializeField] private Shared<Vector3> _offset;

        public LineData(Shared<Vector3> start, Shared<Vector3> offset)
        {
            _start = start;
            _offset = offset;
        }

        public static readonly LineData Default = new LineData(new Shared<Vector3>(), new Shared<Vector3>());
    }

    [System.Serializable]
    public class Line : IShape
    {
        public int Count => _collection?.Count ?? 0;

        public List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new List<Transform>();

        public Vector3 Start => _lineData.Start;
        public Vector3 Offset => _lineData.Offset;

        public IShapeData ShapeData => _lineData;

        [SerializeField] private LineData _lineData;

        public Line()
        {
            _lineData = new();
        }

        public Line(Vector3 offset)
        {
            _lineData = new(new Shared<Vector3>(), new Shared<Vector3>(offset));
        }

        public Line(Vector3 start, Vector3 offset)
        {
            _lineData = new LineData(new Shared<Vector3>(start), new Shared<Vector3>(offset));
        }

        public void AddTransform(Transform xform)
        {
            _collection.Add(xform);
        }

        // Validate the line before calling this. 
        public static void Refresh(List<Transform> transforms, Vector3 start, Vector3 offset)
        {
            int numObjects = transforms.Count;

            Transform current = null;

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
            Refresh(Collection, Start, Offset);
        }

        public static Vector3 GetPositionAtIndex(int i, Vector3 start, Vector3 offset)
        {
            return start + (offset * i);
        }

        public Vector3 GetDefaultPositionAtIndex(int index, Line line)
        {
            var (start, offset) = line;
            return start + (offset * index);
        }

        public void Deconstruct(out Vector3 start, out Vector3 offset)
        {
            start = Start;
            offset = Offset;
        }

        public Vector3 GetDefaultPositionAtIndex(int index)
        {
            return Start + (Offset * index);
        }

        public void SetShapeData(IShapeData shapeData)
        {
            _lineData = (LineData)shapeData;
        }

        public void SetOffset(Vector3 offset)
        {
            _lineData.Offset.Set(offset);
        }

        public void SetStart(Vector3 start)
        {
            _lineData.Start.Set(start);
        }

        public IShapeData GetShapeData() => _lineData;
    }
}