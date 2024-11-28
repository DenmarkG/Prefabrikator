using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [ExecuteInEditMode]
    public class LinearDuplicator : LinearComponent
    {
        [SerializeField] private GameObject _original;

        public int Count
        {
            get => _count;
            set => _count = value;
        }
        
        [SerializeField] private int _count;
        [SerializeField] private bool _keepOriginal;

        public override void Refresh()
        {
            Line.Refresh(Objects, LineInternal);
        }

        public override void AddTransform(Transform xForm = null)
        {
            return;
        }
    }
}