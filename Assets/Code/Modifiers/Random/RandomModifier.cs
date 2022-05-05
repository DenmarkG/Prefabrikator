using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class RandomModifier : Modifier
    {
        protected Shared<Vector3> _min = new Shared<Vector3>();
        protected Shared<Vector3> _max = new Shared<Vector3>();
        protected Vector3Property _minProperty = null;
        protected Vector3Property _maxProperty = null;

        public RandomModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }
    }
}