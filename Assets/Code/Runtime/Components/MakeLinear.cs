using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    using Shapes;
    using System.Diagnostics.Contracts;

    [ExecuteInEditMode]
    public class MakeLinear : MonoBehaviour
    {
        private static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public Vector3 Offset => _offset;
        [SerializeField] private Shared<Vector3> _offset = new(DefaultOffset);

        public Vector3 StartPosition => _start;
        [SerializeField] private Shared<Vector3> _start = new(); 

        [SerializeField] private List<Transform> _objects = null;

        private void Start()
        {
            _offset.OnValueChanged += OnValueChanged;
            _start.OnValueChanged += OnValueChanged;
        }

        private void Update()
        {
            _objects ??= new List<Transform>();
        }

        private void OnValidate()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            _offset.OnValueChanged -= OnValueChanged;
            _start.OnValueChanged -= OnValueChanged;
        }

        public void Refresh()
        {
            if (_objects != null && Line.IsValid(_objects))
            {
                // #DG: Throws error when any item is null. Need to only call when all objects are valid
                Line.Refresh(_objects, _start, _offset);
            }
        }

        private void OnValueChanged(Vector3 value)
        {
            Refresh();
        }

        public void AddTransform(Transform xform)
        {
            if (xform != null)
            {
                _objects.Add(xform);
            }
        }

        public void SetOffset(Vector3 offset)
        {
            _offset.Set(offset);
        }

        public Shared<Vector3> GetSharedOffset() => _offset;
        public Shared<Vector3> GetShareStart() => _start;



#if UNITY_EDITOR

        private bool _isInitialized = false;
        private void OnEnable()
        {
            if (_isInitialized)
            {
                return;
            }

            _start.Set(this.transform.position);
            _isInitialized = true;
        }

        [ContextMenu("Reset Positions")]
        private void Reset()
        {
            _offset.Set(DefaultOffset);
            _start.Set(this.transform.position);
            _isInitialized = false;
        }

        public void InitFromSharedData(Shared<Vector3> start, Shared<Vector3> offset)
        {
            _start = start;
            _offset = offset;
        }

#endif // UNITY_EDITOR
    }
}
