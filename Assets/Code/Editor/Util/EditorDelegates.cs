using UnityEngine;

namespace Prefabrikator.Editor
{
    public delegate void ApplicatorDelegate(IShape target, Transform go);
    public delegate void IndexedApplicatorDelegate(IShape target, Transform go, int index);
}