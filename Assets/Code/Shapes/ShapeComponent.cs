using UnityEngine;

namespace Prefabrikator.Shapes
{
    public abstract class ShapeComponent : MonoBehaviour
    {
        public abstract BaseShape Shape { get; }

        private void OnValidate()
        {
            Shape?.Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            Shape?.Refresh();
        }
    }
}