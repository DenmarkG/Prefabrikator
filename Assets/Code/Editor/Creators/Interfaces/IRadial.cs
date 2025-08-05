using UnityEngine;

namespace Prefabrikator
{
    [SerializeField]
    public interface IRadial
    {
        float Radius { get; }
        Vector3 Center { get; }
    }
}