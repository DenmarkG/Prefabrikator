using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformRotation : UniformModifier
    {
        public override string DisplayName => "Uniform Rotation";

        public UniformRotation(IShape owner)
            : base(owner, "Rotation", 0f)
        {
            //
        }

        protected override void RestoreDefault(IShape target, Transform obj)
        {
            Quaternion defaultRotation = target.GetDefaultRotation();
            obj.transform.rotation = defaultRotation;
        }

        protected override void ApplyModifier(IShape target, TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                proxies[i].Rotation *= Quaternion.Euler(_uniformValue);
            }
        }
    }
}
