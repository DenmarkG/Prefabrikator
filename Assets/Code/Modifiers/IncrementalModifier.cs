using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class IncrementalModifier : Modifier
    {
        public IncrementalModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }

        public override void Process(GameObject[] objs)
        {
            //
        }

        protected override void OnInspectorUpdate()
        {
            //
        }
    }
}
