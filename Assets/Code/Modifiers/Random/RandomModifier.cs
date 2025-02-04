using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class RandomModifier<T> : Modifier where T : struct
    {
        [SerializeField] protected Shared<T> _min = new Shared<T>();
        [SerializeField] protected Shared<T> _max = new Shared<T>();

        public RandomModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }

        protected abstract void Randomize(int startingIndex = 0);
    }
}