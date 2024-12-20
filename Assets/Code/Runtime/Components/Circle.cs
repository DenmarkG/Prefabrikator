using Prefabrikator.Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public class Circle : MonoBehaviour, IShape
    {
        // #DG: TODO: make these tunable
        private static readonly int DefaultMaxCount = 100;
        private static readonly float DefaultRadius = 1f;

        [SerializeField] private Shared<float> _radius = new(DefaultRadius);
        [SerializeField] private Shared<Vector3> _center = new();

        public int MaxCount => _maxCount;
        [SerializeField] private int _maxCount = ShapeHelpers.DefaultMaxCount;
        public int MinCount => 0;

        public List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new();

        public List<Modifier> Modidfiers => throw new System.NotImplementedException();

        public void AddTransform(Transform xform)
        {
            _collection.Add(xform);
            Refresh();
        }

        public Vector3 GetDefaultPositionAtIndex(int index)
        {
            return GetDefaultPositionAtIndex(index, _collection.Count, _radius, _center);
        }

        public static Vector3 GetDefaultPositionAtIndex(int index, int count, float radius, Vector3 center)
        {
            if (count <= 0)
                return center; // #DG: make this count = 1 instead? 

            const float degrees = Mathf.PI * 2;
            float angle = (degrees / count);

            float t = angle * index;
            float x = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;

            return new Vector3(x, 0f, z) + center;
        }

        public void Refresh()
        {
            for (int i = 0; i < _collection.Count; ++i)
            {
                var xform = _collection[i].transform;
                if (xform != null)
                {
                    xform.localPosition = GetDefaultPositionAtIndex(i);
                }
            }
        }

#if UNITY_EDITOR

        [ContextMenu("Reset Center")]
        private void ResetStart()
        {
            _center = new Shared<Vector3>(this.transform.position);
        }

        private void OnValidate()
        {
            _collection ??= new();
            Refresh();
        }
#endif // UNITY_EDITOR
    }
}