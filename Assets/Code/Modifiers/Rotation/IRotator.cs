using UnityEngine;

namespace Prefabrikator
{
    interface IRotator
    {
        Quaternion GetRotationAtIndex(int index);
    }
}
