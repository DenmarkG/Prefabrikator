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

        protected override void ApplyModifier()
        {
            Quaternion rotation = Quaternion.Euler(_target);
            Owner.ApplyToAll((go) => { go.transform.rotation = rotation; });
        }
    }
}
