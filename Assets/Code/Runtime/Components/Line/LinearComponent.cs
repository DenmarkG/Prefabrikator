using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public abstract class LinearComponent : PrefabrikatorComponent
    {
        protected static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public Vector3 Offset => _lineData.Offset;
        public Vector3 StartPosition => _lineData.Start;
        public override IShapeData ShapeData => _lineData;

        protected LineData LineInternal => _lineData;
        [SerializeField] private LineData _lineData = new();

        protected abstract List<Transform> Objects { get; }

        public ref Vector3 GetSharedOffset() => ref _lineData.Offset;
        public ref Vector3 GetShareStart() => ref _lineData.Start;

        public void SetOffset(Vector3 offset)
        {
            _lineData.Offset = offset;
        }

        public abstract void AddTransform(Transform xForm = null);

        protected void AddTransformInternal(Transform xform)
        {
            if (xform != null)
            {
                Objects.Add(xform);
                Refresh();
            }
        }

#if UNITY_EDITOR

        private bool _isInitialized = false;
        private void OnEnable()
        {
            if (_isInitialized)
            {
                return;
            }

            _lineData.Start = this.transform.position;
            _isInitialized = true;
        }

        [ContextMenu("Reset Positions")]
        private void Reset()
        {
            _lineData.Offset = DefaultOffset;
            _lineData.Start = this.transform.position;
            _isInitialized = false;
        }

        public void InitFromSharedData(Shared<Vector3> start, Shared<Vector3> offset)
        {
            _lineData.Start = start;
            _lineData.Offset = offset;
        }

#endif // UNITY_EDITOR
    }
}