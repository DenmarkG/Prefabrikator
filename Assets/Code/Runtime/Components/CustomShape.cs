using Prefabrikator.Shapes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Prefabrikator.Runtime
{





    public class CustomShape : MonoBehaviour, IShape
    {
        //[SerializeField] private CustomShapeMode _mode = CustomShapeMode.Duplicate;

        [SerializeField] private IShapeData _shapeData; // #DG: this won't work. Need to rebulid from list of properties instead

        public GameObject Seletion => _selection;
        [SerializeField] private GameObject _selection;

        public int Count => _collection?.Count ?? 0;
        
        public List<Transform> Collection => _collection;
        [SerializeField][HideInInspector] private List<Transform> _collection = new();
        [SerializeField][HideInInspector] private List<TransformProxy> _originalTransforms = new();

        public ShapeType BaseShape => _baseShape;

        public IShapeData ShapeData => throw new NotImplementedException();

        [SerializeField] private ShapeType _baseShape = ShapeType.Line;

        [SerializeField][HideInInspector] private UnityEvent<int> _getPositionAtIndex = new();

        public void AddTransform(Transform xform)
        {
            _collection.Add(xform);
        }

        public Vector3 GetDefaultPositionAtIndex(int index)
        {
            if (index < 0 || index > _originalTransforms.Count)
                throw new ShapeException("Attempting to get the position of a shape with an invalid index");

            return _originalTransforms[index].Position;
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SetSelection(GameObject selection)
        {
            _selection = selection;
        }

        public IShapeData GetShapeData()
        {
            return _shapeData ??= ShapeHelpers.CreateDefaultData(_baseShape);
        }

        public void SetShapeData(IShapeData shapeData)
        {
            _shapeData = shapeData;
        }
    }
}
