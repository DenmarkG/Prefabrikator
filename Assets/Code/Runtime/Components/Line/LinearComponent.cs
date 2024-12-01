using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public abstract class LinearComponent : PrefabrikatorComponent
    {
        public Vector3 Offset => _line.ShapeData.Offset;
        public Vector3 StartPosition => _line.ShapeData.Start;
        public override IShapeData ShapeData => _line.ShapeData;

        protected List<Transform> Objects => _line.Collection;

        public Line LineInternal => _line;
        [SerializeField] private Line _line = new Line();

        // #DG: rework this
        //public ref Vector3 GetSharedOffset() => ref _line.ShapeData.Offset;
        //public ref Vector3 GetShareStart() => ref _lineData.Start;

        public void SetOffset(Vector3 offset)
        {
            _line.SetOffset(offset);
        }

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

            _line.SetStart(this.transform.position);
            _isInitialized = true;
        }

        [ContextMenu("Reset Positions")]
        private void Reset()
        {
            _line = new Line(this.transform.position);
            _isInitialized = false;
        }

#endif // UNITY_EDITOR
    }
}