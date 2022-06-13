using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class IncrementalModifier : Modifier
    {
        protected Shared<Vector3> Target { get; private set; }
        private Vector3Property _targetProperty = null;

        //private bool _reverseDirection = false;

        public IncrementalModifier(ArrayCreator owner, Vector3 defaultValue)
            : base(owner)
        {
            Target = new Shared<Vector3>(defaultValue);

            void OnTargetChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(Target, previous, current));
            }
            _targetProperty = new Vector3Property("Target Rotation", Target, OnTargetChanged);
        }

        protected override void OnInspectorUpdate()
        {
            Target.Set(_targetProperty.Update());
        }
    }
}
