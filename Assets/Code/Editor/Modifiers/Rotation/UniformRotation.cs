using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformRotation : UniformModifier
    {
        protected override string DisplayName => "Uniform Rotation";

        public UniformRotation(ArrayCreator owner)
            : base(owner, "Rotation", 0f)
        {
            //
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
                proxies[i].Rotation *= Quaternion.Euler(_target);
            }
        }
    }
}
