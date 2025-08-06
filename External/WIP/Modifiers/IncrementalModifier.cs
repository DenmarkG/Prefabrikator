using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class IncrementalModifier : Modifier
    {
        protected Shared<Vector3> IncrementValue { get; private set; }

        //private bool _reverseDirection = false;

        public IncrementalModifier(IRuntimeShape target, Vector3 defaultValue)
        {
            IncrementValue = new Shared<Vector3>(defaultValue);
        }
    }
}
