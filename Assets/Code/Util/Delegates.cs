using Prefabrikator.Runtime;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public delegate void RuntimeApplicatorDelegate(IRuntimeShape target, Transform go);
    public delegate void RuntimeIndexedApplicatorDelegate(IRuntimeShape runtimeShape, Transform proxy, int index);
}