using UnityEngine;

namespace Prefabrikator
{
    public delegate void ApplicatorDelegate(IShape target, Transform go);
    public delegate void IndexedApplicatorDelegate(IShape target, Transform go, int index);
}