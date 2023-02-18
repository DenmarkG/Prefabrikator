using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class ArrayContainer : MonoBehaviour
    {
        public ArrayState Data => _data;
        [SerializeReference] private ArrayState _data = null;

        public void SetData(ArrayState data)
        {
            _data = data;
        }
    }
}