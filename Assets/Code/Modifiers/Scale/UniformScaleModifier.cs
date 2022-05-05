using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformScaleModifier : Modifier
    {
        protected override string DisplayName => "Uniform Scale";

        public GameObject[] Targets => _targets;
        private GameObject[] _targets = null;
        private Shared<Vector3> _targetScale = new Shared<Vector3>(new Vector3(1f, 1f, 1f));
        private Vector3Property _targetScaleProperty = null;

        public UniformScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            _targetScaleProperty = new Vector3Property("Scale", _targetScale, OnValueChanged);
        }

        public override void Process(GameObject[] objs)
        {
            if (_targets == null || objs.Length != _targets.Length)
            {
                _targets = objs;
            }

            Vector3 scale = _targetScale;
            Owner.ApplyToAll((go) => { go.transform.localScale = scale; });
        }

        protected override void OnInspectorUpdate()
        {
            _targetScale.Set(_targetScaleProperty.Update());
        }

        public void OnValueChanged(Vector3 current, Vector3 previous)
        {
            Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_targetScale, previous, current));
        }

        public override void OnRemoved()
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            Owner.ApplyToAll((go) => { go.transform.localScale = defaultScale; });
        }
    }
}
