using Prefabrikator.Shapes;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public class CircleComponent : ShapeComponent
    {
        public sealed override IShape Shape => _circle;
        [SerializeField] private Circle _circle = new();

#if UNITY_EDITOR

        [ContextMenu("Reset Center")]
        private void ResetStart()
        {
            _circle.Center = this.transform.position;
        }

        private void OnValidate()
        {
            _circle.Refresh();
        }

#endif // UNITY_EDITOR
    }
}