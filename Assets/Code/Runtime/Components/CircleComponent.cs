using Prefabrikator.Shapes;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public class CircleComponent : ShapeComponent
    {
        public sealed override BaseShape Shape => _circle;
        [SerializeField] private Circle _circle = new();

#if UNITY_EDITOR

        [ContextMenu("Reset Center")]
        private void ResetStart()
        {
            _circle.Center = this.transform.position;
        }

#endif // UNITY_EDITOR
    }
}