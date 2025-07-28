using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalScaleModifier : IncrementalModifier
    {
        public override string DisplayName => "Incremental Scale";

        public IncrementalScaleModifier(IShape owner)
            : base(owner, new Vector3(2f, 2f, 2f))
        {
            //
        }
        
        public override TransformProxy[] Process(IShape target, TransformProxy[] proxies)
        {
            // #DG: make this account for change to starting scale
            Vector3 defaultScale = target.GetDefaultScale();
            int numObjs = proxies.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Vector3 scale = Vector3.Lerp(defaultScale, Target, t);
                proxies[i].Scale = scale;
            }

            return proxies;
        }

        public override void OnRemoved(IShape target)
        {
            Teardown(target);
        }

        public override void Teardown(IShape target)
        {
            Vector3 defaultScale = target.GetDefaultScale();
            target.ApplyToAll((_, go) => { go.transform.localScale = defaultScale; });
        }
    }
}
