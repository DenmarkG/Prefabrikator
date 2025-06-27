using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public abstract class ShapeComponent : MonoBehaviour
    {
        public abstract List<Transform> Collection { get; }
        public abstract List<Modifier> Modifiers { get; }
        public abstract void OnRefresh();

        public int Count => Collection?.Count ?? 0;

        public void AddTransform(Transform xform)
        {
            Collection.Add(xform);
            Refresh();
        }

        private void OnValidate()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            OnRefresh();
        }
    }
}