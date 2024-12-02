using System.Collections;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalRotationModifier : IncrementalModifier
    {
        protected override string DisplayName => "Incremental Rotation";

        public IncrementalRotationModifier(ArrayCreator owner)
            : base(owner, new Vector3(0f, 90f, 0f))
        {
            //
        }

        public override TransformProxy[] Process(TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;
            // #DG: make this account for changes to the starting rotation (uniform mod)
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Quaternion rotation = Quaternion.Lerp(defaultRotation, Quaternion.Euler(Target), t);
                proxies[i].Rotation = rotation;
            }

            return proxies;
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Teardown()
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }
    }
}
