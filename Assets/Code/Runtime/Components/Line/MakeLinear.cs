using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    using Shapes;

    [ExecuteInEditMode]
    public class MakeLinear : LinearComponent
    {
        protected override List<Transform> Objects => _objects;
        [SerializeField] private List<Transform> _objects = new();

        private void OnValidate()
        {
            Refresh();
        }

        public override void Refresh()
        {
            if (Objects != null && Line.IsValid(Objects))
            {
                Line.Refresh(Objects, LineInternal);
            }
        }

        public override void AddTransform(Transform xForm = null)
        {
            AddTransform(xForm);
        }
    }
}
