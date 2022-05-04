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
        private Shared<Vector3> _targetRotation = new Shared<Vector3>(new Vector3(1f, 1f, 1f));
        private Vector3Property _targetRotationProperty = null;

        public UniformRotation(ArrayCreator owner)
            : base(owner)
        {
            //_targetRotationProperty = new Vector3Property("Rotation", _targetRotation, OnValueChange);
        }

        public override void OnRemoved()
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }

        public override void Process(GameObject[] objs)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInspectorUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void OnValueChanged(Vector3 current, Vector3 previous)
        {
            //Owner.CommandQueue.Enqueue(new OnUniformScaleChangeCommand(this, previous, current));
        }
    }
}
