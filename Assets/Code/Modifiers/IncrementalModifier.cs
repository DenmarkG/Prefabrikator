using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class IncrementalModifier : Modifier
    {
        protected Shared<Vector3> Target { get; private set; }
        [SerializeField] private Vector3Property _targetProperty = null;

        //private bool _reverseDirection = false;

        public IncrementalModifier(IShape target, Vector3 defaultValue)
        {
            Target = new Shared<Vector3>(defaultValue);

            void OnTargetChanged(Vector3 current, Vector3 previous)
            {
                target.CommandQueue.Enqueue(new GenericCommand<Vector3>(Target, previous, current));
            }
            _targetProperty = new Vector3Property("Target", Target, OnTargetChanged);
        }

        protected override void OnInspectorUpdate(IShape target)
        {
            Target.Set(_targetProperty.Update());
        }
    }
}
