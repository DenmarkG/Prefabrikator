using System.Collections;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalRotationModifier : IncrementalModifier
    {
        public override string DisplayName => "Incremental Rotation";

        public IncrementalRotationModifier(IShape owner)
            : base(owner, new Vector3(0f, 90f, 0f))
        {
            //
        }

        public override TransformProxy[] Process(IShape target, TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;
            // #DG: make this account for changes to the starting rotation (uniform mod)
            Quaternion defaultRotation = target.GetDefaultRotation();
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Quaternion rotation = Quaternion.Lerp(defaultRotation, Quaternion.Euler(Target), t);
                proxies[i].Rotation = rotation;
            }

            return proxies;
        }

        public override void OnRemoved(IShape target)
        {
            Teardown(target);
        }

        public override void Teardown(IShape target)
        {
            Quaternion defaultRotation = target.GetDefaultRotation();
            target.ApplyToAll((_, go) => { go.transform.rotation = defaultRotation; });
        }
    }
}
