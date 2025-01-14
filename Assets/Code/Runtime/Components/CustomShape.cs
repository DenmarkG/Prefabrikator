using Prefabrikator.Shapes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Prefabrikator.Runtime
{
    public class CustomShape : ShapeComponent
    {
        //[SerializeField] private CustomShapeMode _mode = CustomShapeMode.Duplicate;

        public GameObject Seletion => _selection;
        [SerializeField] private GameObject _selection;

        public override BaseShape Shape => _shape;
        [SerializeReference] private BaseShape _shape = new Line(LineData.Default);

        [SerializeField] private string _serialized = string.Empty;

        public int Count => _collection?.Count ?? 0;
        
        public List<Transform> Collection => _collection;
        [SerializeField][HideInInspector] private List<Transform> _collection = new();

        public ShapeType BaseShapeType => _shapeType;
        [SerializeField] private ShapeType _shapeType = ShapeType.Line;

        public void AddTransform(Transform xform)
        {
            _collection.Add(xform);
        }

        public Vector3 GetDefaultPositionAtIndex(int index)
        {
            if (index < 0 || index > _collection.Count)
                throw new IndexOutOfRangeException();

            return _shape.GetDefaultPositionAtIndex(index);
        }

        public void SetSelection(GameObject selection)
        {
            _selection = selection;
        }

        public IShapeData GetShapeData()
        {
            return Shape?.ShapeData;
        }

#if UNITY_EDITOR

        private ShapeType _cachedShape;
        private void OnValidate()
        {
            if (_cachedShape != _shapeType)
            {
                _shape = ShapeFactory.CreateShape(_shapeType);
                _cachedShape = _shapeType;
            }

            Refresh();
        }
#endif // UNITY_EDITOR
    }
}
