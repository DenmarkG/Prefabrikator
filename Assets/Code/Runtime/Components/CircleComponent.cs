using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public class CircleComponent : ShapeComponent
    {
        private static readonly float DefaultRadius = 1f;
        [SerializeField] private Circle _circle = new() { Radius = DefaultRadius };

        public float Radius => _circle.Radius;

        public Vector3 Center => _circle.Center;

        public override List<Transform> Collection => _collection;
        [SerializeField] private List<Transform> _collection = new();

        public override List<Modifier> Modifiers => _modifiers;


        [SerializeField] private List<Modifier> _modifiers = new();


        public override void OnRefresh()
        {
            _circle.Refresh(_collection);
        }

        public override IShape GetShapeData() => _circle;

#if UNITY_EDITOR

        [ContextMenu("Reset Center")]
        private void ResetStart()
        {
            _circle.Center = this.transform.position;
        }

#endif // UNITY_EDITOR
    }
}