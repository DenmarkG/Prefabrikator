using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Prefabrikator.Runtime
{
    [ExecuteInEditMode]
    public class LineComponent : ShapeComponent
    {
        public static readonly Vector3 DefaultOffset = new Vector3(2f, 0f, 0f);

        public Vector3 Start => _line.Start;
        public Vector3 Offset => _line.Offset;
        [SerializeField] private Line _line = new Line();
        
        public override List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new List<Transform>();

        public override List<Modifier> Modifiers => _modifiers;


        [SerializeReference] private List<Modifier> _modifiers = new();


        public void SetOffset(Vector3 offset)
        {
            _line.Offset = offset;
        }

        public void SetStart(Vector3 start)
        {
            _line.Start = start;
        }

        public override void OnRefresh()
        {
            _line.Refresh(Collection);
        }


#if UNITY_EDITOR

        public override IRuntimeShape GetShapeData() => _line;

        public Shared<Vector3> SharedStart => _sharedStart ??= new(Start, OnSetStart);
        private Shared<Vector3> _sharedStart;

        public Shared<Vector3> SharedOffset => _sharedOffset ??= new(Offset, OnSetOffset);
        private Shared<Vector3> _sharedOffset;

        private void OnSetOffset(Vector3 offset) => _line.Offset = offset;
        private void OnSetStart(Vector3 start) => _line.Start = start;

        [ContextMenu("Reset Start")]
        private void ResetStart()
        {
            _line.Start = this.transform.position;
        }

        public void ResetSharedData()
        {
            _sharedStart = new(Start, OnSetStart);
            _sharedOffset = new(Offset, OnSetOffset);
        }

#endif // UNITY_EDITOR
    }
}