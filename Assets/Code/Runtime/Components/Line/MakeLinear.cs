using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    using Shapes;

    [ExecuteInEditMode]
    public class MakeLinear : LinearComponent
    {
        private void OnValidate()
        {
            Refresh();
        }

        public override void Refresh()
        {
            if (Objects != null && Line.IsValid(Objects))
            {
                // #DG: Throws error when any item is null. Need to only call when all objects are valid
                Line.Refresh(Objects, LineInternal);
            }
        }

        public override void AddTransform(Transform xForm = null)
        {
            AddTransform(xForm);
        }
    }
}
