using Prefabrikator.Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [System.Serializable]
    public struct CircleData : IShapeData
    {
        public Shared<Vector3> Center => _center;
        [SerializeField] private Shared<Vector3> _center;

        public Shared<float> Radius => _radius;
        [SerializeField] private Shared<float> _radius;

        public CircleData(Shared<float> radius)
        {
            _center = new Shared<Vector3>();
            _radius = radius;
        }

        public CircleData(Shared<Vector3> center, Shared<float> radius)
        {
            _center = center;
            _radius = radius;
        }

        public static readonly CircleData Default = new CircleData(new Shared<Vector3>(), new Shared<float>());
    }

    [System.Serializable]
    public class Circle : BaseShape
    {
        private static readonly float DefaultRadius = 1f;

        public float Radius
        {
            get { return _circleData.Radius; }
            set { _circleData.Radius.Set(value); }
        }
        
        public Vector3 Center
        {
            get { return _circleData.Center; }
            set { _circleData.Center.Set(value); }
        }

        public override int Count => _collection?.Count ?? 0;

        public override List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new();

        //public List<Modifier> Modidfiers => throw new System.NotImplementedException();

        public override IShapeData ShapeData => _circleData;
        [SerializeField] private CircleData _circleData = new(new Shared<float>(DefaultRadius));

        public override void AddTransform(Transform xform)
        {
            _collection.Add(xform);
            Refresh();
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return GetDefaultPositionAtIndex(index, _collection.Count, Radius, Center);
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

        public override void Refresh()
        {
            if (_collection != null)
            {
                for (int i = 0; i < _collection.Count; ++i)
                {
                    var xform = _collection[i]?.transform;
                    if (xform != null)
                    {
                        xform.localPosition = GetDefaultPositionAtIndex(i);
                    }
                }
            }
        }

        public override IShapeData GetShapeData()
        {
            return _circleData;
        }

        public override void SetShapeData(IShapeData shapeData)
        {
            if (shapeData is CircleData circle)
            {
                Center = circle.Center;
                Radius = circle.Radius;
            }
        }
    }
}