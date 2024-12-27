using Prefabrikator.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public class CustomShape : MonoBehaviour, IShape
    {
        [SerializeField] private CustomShapeMode _mode = CustomShapeMode.Duplicate;


        public GameObject Seletion => _selection;
        [SerializeField] private GameObject _selection;

        public int Count => _collection?.Count ?? 0;
        
        public List<Transform> Collection => _collection;
        [SerializeField][HideInInspector] private List<Transform> _collection = new();

        [SerializeField][HideInInspector] private List<TransformProxy> _originalTransforms = new();

        public ShapeType BaseShape => _baseShape;
        [SerializeField][HideInInspector] private ShapeType _baseShape = ShapeType.Line;

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
    }
}
