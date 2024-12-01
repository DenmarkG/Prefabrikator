using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    using Shapes;

    [ExecuteInEditMode]
    public class MakeLinear : LinearComponent
    {
        [SerializeField] protected List<Transform> _transforms;

        private void OnValidate()
        {
            Refresh();
        }

        public override void Refresh()
        {
            LineInternal.SetTransforms(_transforms);
            LineInternal.Refresh();
        }

        public void AddTransform(Transform xForm = null)
        {
            _transforms.Add(xForm);
            LineInternal.AddTranform(xForm);
        }
    }
}
