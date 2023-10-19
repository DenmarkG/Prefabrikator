using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformRotation : UniformModifier
    {
        protected override string DisplayName => "Uniform Rotation";

        public UniformRotation(ArrayCreator owner)
            : base(owner)
        {
            _target = new Shared<Vector3>(new Vector3());
            _targetProperty = new Vector3Property("Rotation", _target, OnValueChanged);
        }

        protected override void RestoreDefault(GameObject obj)
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            obj.transform.rotation = defaultRotation;
        }

        protected override void ApplyModifier(TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                proxies[i].Scale += _target;
            }
        }
    }
}
