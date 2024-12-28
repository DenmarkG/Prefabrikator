using Prefabrikator.Shapes;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [System.Serializable]
    public struct LineData : IShapeData
    {
        public Shared<Vector3> Start { get; }
        public Shared<Vector3> Offset { get; }

        public LineData(Shared<Vector3> start, Shared<Vector3> offset)
        {
            Start = start;
            Offset = offset;
        }

        public static readonly LineData Default = new LineData(new Shared<Vector3>(), new Shared<Vector3>());
    }

    [ExecuteInEditMode]
    public class Line : MonoBehaviour, IShape
    {
        public static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public Vector3 Start => _start;
        [SerializeField] private Shared<Vector3> _start = new();
        public Vector3 Offset => _offset;
        [SerializeField] private Shared<Vector3> _offset = new();

        public int Count => _collection?.Count ?? 0;

        public List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new List<Transform>();

        public List<Modifier> Modidfiers => new List<Modifier>();

        public void SetOffset(Vector3 offset)
        {
            _offset.Set(offset);
        }

        public void SetStart(Vector3 start)
        {
            _start.Set(start);
        }

        public void AddTransform(Transform xform)
        {
            _collection.Add(xform);
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
            Refresh(Collection, _start, _offset);
        }

        public void Deconstruct(out Vector3 start, out Vector3 offset)
        {
            start = Start;
            offset = Offset;
        }

        public Vector3 GetDefaultPositionAtIndex(int index)
        {
            return _start.Get() + (_offset.Get() * index);
        }

        public void SetShapeData(IShapeData ShapeData)
        {
            if (ShapeData is LineData lineData)
            {
                _start = lineData.Start;
                _offset = lineData.Offset;
            }
        }

        public IShapeData GetShapeData()
        {
            return new LineData(_start, _offset);
        }

#if UNITY_EDITOR

        [ContextMenu("Reset Start")]
        private void ResetStart()
        {
            _start = new Shared<Vector3>(this.transform.position);
        }

        private void OnValidate()
        {
            Refresh();
        }

#endif // UNITY_EDITOR
    }
}