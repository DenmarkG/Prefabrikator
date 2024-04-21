using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    using Shapes;

    [ExecuteInEditMode]
    public class LineArray : MonoBehaviour
    {
        [SerializeField] private Shared<Vector3> _offset = new(new Vector3(2f, 0f, 0f));
        [SerializeField] private Shared<Vector3> _start = new(); 
        [SerializeField] private List<Transform> _objects = null;
        
        private Transform _transform = null;

        private void Awake()
        {
            _transform = this.transform;
        }

        private void Update()
        {
            _objects ??= new List<Transform>();
            _transform ??= this.transform; // #DG: edit time only
        }

        private void OnValidate()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_objects != null)
            {
                // #DG: Throws error when any item is null. Need to only call when all objects are valid
                Line.Refresh(_objects, _start, _offset);
            }
        }

#if UNITY_EDITOR
        public void InitFromSharedData(Shared<Vector3> start, Shared<Vector3> offset)
        {
            _start = start;
            _offset = offset;
        }
#endif // UNITY_EDITOR
    }
}
