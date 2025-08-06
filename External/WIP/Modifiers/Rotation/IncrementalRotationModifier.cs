using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalRotationModifier : IncrementalModifier
    {
        public override string DisplayName => "Incremental Rotation";


        private Quaternion Rotation => Quaternion.Euler(IncrementValue);

        public IncrementalRotationModifier(IRuntimeShape owner)
            : base(owner, new Vector3(0f, 90f, 0f))
        {
        }

        public override TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            if (proxies == null || (proxies != null && proxies.Length == 0))
                return proxies;

            int numObjs = proxies.Length;
            // #DG: make this account for changes to the starting rotation (uniform mod)
            Quaternion defaultRotation = proxies[0].Rotation;
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Quaternion rotation = Quaternion.Lerp(defaultRotation, Quaternion.Euler(IncrementValue), t);
                proxies[i].Rotation *= rotation;
            }

            return proxies;
        }

        public override void OnRemoved(IRuntimeShape target, Transform[] proxies)
        {
            Teardown(target, proxies);
        }

        // #DG: pass in default info? 
        public override void Teardown(IRuntimeShape target, Transform[] proxies)
        {
            if (proxies == null || (proxies != null && proxies.Length == 0))
                return;

            int count = proxies.Length;

            Quaternion defaultRotation = proxies[0].rotation;

            // #DG: apply the inverse of the transform of this modifier
            target.ApplyToAll(proxies, (shape, proxy, index) => { proxy.rotation = defaultRotation; });
        }
    }
}
