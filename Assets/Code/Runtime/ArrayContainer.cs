using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class ArrayContainer : MonoBehaviour
    {
        public ArrayData Data => _data;
        [SerializeReference] private ArrayData _data = null;

        public void SetData(ArrayData data)
        {
            _data = data;
        }
    }
}