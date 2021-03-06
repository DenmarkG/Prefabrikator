using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformScaleModifier : UniformModifier
    {
        protected override string DisplayName => "Uniform Scale";

        public UniformScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            _target = new Shared<Vector3>(new Vector3(1f, 1f, 1f));
            _targetProperty = new Vector3Property("Scale", _target, OnValueChanged);
        }

        protected override void RestoreDefault(GameObject obj)
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            obj.transform.localScale = defaultScale;
        }

        protected override void ApplyModifier()
        {
            Vector3 scale = _target;
            Owner.ApplyToAll((go) => { go.transform.localScale = scale; });
        }
    }
}
