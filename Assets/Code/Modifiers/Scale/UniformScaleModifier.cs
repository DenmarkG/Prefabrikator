using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformScaleModifier : UniformModifier
    {
        public override string DisplayName => "Uniform Scale";

        public UniformScaleModifier(IRuntimeShape target)
            : base(target, "Scale", 1f)
        {
            //
        }

        protected override void RestoreDefault(IRuntimeShape target, Transform obj)
        {
            Vector3 defaultScale = target.GetDefaultScale();
            obj.transform.localScale = defaultScale;
        }

        protected override void ApplyModifier(IRuntimeShape target, TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                Vector3 scale = proxies[i].Scale;

                scale.x *= Mathf.Abs(((Vector3)_uniformValue).x);
                scale.y *= Mathf.Abs(((Vector3)_uniformValue).y);
                scale.z *= Mathf.Abs(((Vector3)_uniformValue).z);

                proxies[i].Scale = scale;
            }
        }
    }
}
