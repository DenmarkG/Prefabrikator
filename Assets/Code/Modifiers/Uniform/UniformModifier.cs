using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class UniformModifier : Modifier
    {
        protected GameObject[] _targets = null;
        protected Shared<Vector3> _target = null;
        protected Vector3Property _targetProperty = null;

        public UniformModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }

        public override sealed void Process(GameObject[] objs)
        {
            if (_targets == null || objs.Length != _targets.Length)
            {
                _targets = objs;
            }

            ApplyModifier();
        }

        public override sealed void OnRemoved()
        {
            Owner.ApplyToAll(RestoreDefault);
        }

        protected override sealed void OnInspectorUpdate()
        {
            _target.Set(_targetProperty.Update());
        }

        protected void OnValueChanged(Vector3 current, Vector3 previous)
        {
            Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_target, previous, current));
        }

        protected abstract void RestoreDefault(GameObject obj);
        protected abstract void ApplyModifier();
    }
}