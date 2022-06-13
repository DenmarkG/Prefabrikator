using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalScaleModifier : IncrementalModifier
    {
        protected override string DisplayName => "Incremental Scale";

        public IncrementalScaleModifier(ArrayCreator owner)
            : base(owner, new Vector3(2f, 2f, 2f))
        {
            //
        }
        
        public override void Process(GameObject[] objs)
        {
            // #DG: make this account for change to starting scale
            Vector3 defaultScale = Owner.GetDefaultScale();
            int numObjs = objs.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Vector3 scale = Vector3.Lerp(defaultScale, Target, t);
                objs[i].transform.localScale = scale;
            }
        }

        public override void OnRemoved()
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            Owner.ApplyToAll((go) => { go.transform.localScale = defaultScale; });
        }
    }
}
