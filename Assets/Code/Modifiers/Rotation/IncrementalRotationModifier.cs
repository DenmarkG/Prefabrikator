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

        public override void Process(GameObject[] objs)
        {
            int numObjs = objs.Length;
            // #DG: make this account for changes to the starting rotation (uniform mod)
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Quaternion rotation = Quaternion.Lerp(defaultRotation, Quaternion.Euler(Target), t);
                objs[i].transform.rotation = rotation;
            }
        }

        public override void OnRemoved()
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }
    }
}
