using UnityEngine;

namespace Prefabrikator
{
    public static class ToolExtensions
    {
        public static bool IsPrefab(this GameObject obj)
        {
            return string.IsNullOrEmpty(obj.scene.name);
        }
    }
}
