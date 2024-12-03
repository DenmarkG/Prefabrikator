using Prefabrikator.Shapes;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Runtime
{
    [ExecuteInEditMode]
    public class Line : MonoBehaviour, IShape
    {
        public static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public int MaxCount => 50;
        public int MinCount => 0;

        public Vector3 Start => _start;
        public Vector3 Offset => _offset;

        public List<Transform> Collection => new List<Transform>();

        public List<Modifier> Modidfiers => new List<Modifier>();

        [SerializeField] private Shared<Vector3> _start;
        [SerializeField] private Shared<Vector3> _offset;

        public void SetOffset(Vector3 offset)
        {
            _offset.Set(offset);
        }

        public void SetStart(Vector3 start)
        {
            _start.Set(start);
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
            Refresh(Collection, _start, _offset);
        }

        public void Deconstruct(out Vector3 start, out Vector3 offset)
        {
            start = Start;
            offset = Offset;
        }

        public Vector3 GetDefaultPositionAtIndex(int index)
        {
            throw new System.NotImplementedException();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            Refresh();
        }

#endif // UNITY_EDITOR
    }
}