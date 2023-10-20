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

        protected override void ApplyModifier(TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                Vector3 scale = proxies[i].Scale;

                scale.x *= Mathf.Abs(((Vector3)_target).x);
                scale.y *= Mathf.Abs(((Vector3)_target).y);
                scale.z *= Mathf.Abs(((Vector3)_target).z);

                proxies[i].Scale = scale;
            }
        }
    }
}
