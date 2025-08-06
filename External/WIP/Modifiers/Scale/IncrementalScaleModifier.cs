using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalScaleModifier : IncrementalModifier
    {
        public override string DisplayName => "Incremental Scale";

        public IncrementalScaleModifier(IRuntimeShape owner)
            : base(owner, new Vector3(2f, 2f, 2f))
        {
            //
        }
        
        public override TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            if (proxies == null || (proxies != null && proxies.Length == 0))
                return proxies;

            // #DG: make this account for change to starting scale
            Vector3 defaultScale = proxies[0].Scale;
            int numObjs = proxies.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Vector3 scale = Vector3.Lerp(defaultScale, IncrementValue, t);
                proxies[i].Scale = scale;
            }

            return proxies;
        }

        public override void OnRemoved(IRuntimeShape target, Transform[] proxies)
        {
            Teardown(target, proxies);
        }

        public override void Teardown(IRuntimeShape target, Transform[] proxies)
        {
            //Vector3 defaultScale = target.GetDefaultScale();
            //target.ApplyToAll((_, go) => { go.transform.localScale = defaultScale; });
        }
    }
}
