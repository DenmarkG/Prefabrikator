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
        public static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public Shared<Vector3> Start => _start;
        [SerializeField] private Shared<Vector3> _start; 
        
        public Shared<Vector3> Offset => _offset;
        [SerializeField] private Shared<Vector3> _offset;

        public LineData(Shared<Vector3> offset)
        {
            _start = new Shared<Vector3>();
            _offset = offset;
        }

        public LineData(Shared<Vector3> start, Shared<Vector3> offset)
        {
            _start = start;
            _offset = offset;
        }

        public static readonly LineData Default = new LineData(new Shared<Vector3>(), new Shared<Vector3>(DefaultOffset));
    }

    [System.Serializable]
    public class Line : BaseShape
    {
        public override int Count => _collection?.Count ?? 0;

        public override List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new List<Transform>();

        public Vector3 Start => _lineData.Start;
        public Vector3 Offset => _lineData.Offset;

        public override IShapeData ShapeData => _lineData;

        [SerializeField] private LineData _lineData;

        public override List<Modifier> Modifiers => _modifiers;
        [SerializeReference] private List<Modifier> _modifiers = new();

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

        public Line(LineData data)
        {
            _lineData = data;
        }

        public override void AddTransform(Transform xform)
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

        public override void Refresh()
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

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return Start + (Offset * index);
        }

        public override void SetShapeData(IShapeData shapeData)
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

        public override IShapeData GetShapeData() => _lineData;
    }
}