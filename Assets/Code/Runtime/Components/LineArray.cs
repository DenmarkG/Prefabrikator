using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    public class LineArray : MonoBehaviour
    {
        [SerializeField] private Shared<Vector3> _offset = new(new Vector3(2f, 0f, 0f));
        [SerializeField] private Shared<Vector3> _start = new();
        [SerializeField] private Shared<int> _count = new();

#if UNITY_EDITOR
        public void InitFromSharedData(Shared<Vector3> start, Shared<Vector3> offset, Shared<int> count)
        {
            _start = start;
            _offset = offset;
            _count = count;
        }
#endif // UNITY_EDITOR
    }
}
