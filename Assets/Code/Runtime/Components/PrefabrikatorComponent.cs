using Prefabrikator.Shapes;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public abstract class PrefabrikatorComponent : MonoBehaviour
    {
        public abstract IShapeData ShapeData { get; }
        public abstract void Refresh();
    }
}