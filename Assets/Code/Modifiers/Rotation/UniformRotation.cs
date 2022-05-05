using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformRotation : Modifier
    {
        protected override string DisplayName => "Uniform Rotation";

        public GameObject[] Targets => _targets;
        private GameObject[] _targets = null;
        private Shared<Vector3> _targetRotation = new Shared<Vector3>(new Vector3());
        private Vector3Property _targetRotationProperty = null;

        public UniformRotation(ArrayCreator owner)
            : base(owner)
        {
            _targetRotationProperty = new Vector3Property("Rotation", _targetRotation, OnValueChanged);
        }

        public override void OnRemoved()
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }

        public override void Process(GameObject[] objs)
        {
            if (_targets == null || objs.Length != _targets.Length)
            {
                _targets = objs;
            }

            Quaternion rotation = Quaternion.Euler(_targetRotation);
            Owner.ApplyToAll((go) => { go.transform.rotation = rotation; });
        }

        protected override void OnInspectorUpdate()
        {
            _targetRotation.Set(_targetRotationProperty.Update());
        }

        public void OnValueChanged(Vector3 current, Vector3 previous)
        {
            Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_targetRotation, previous, current));
        }
    }
}
