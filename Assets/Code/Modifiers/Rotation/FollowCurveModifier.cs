using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class FollowCurveModifier : Modifier
    {
        protected override string DisplayName => "Follow Curve";

        private CircularArrayCreator _circle = null;
        public FollowCurveModifier(ArrayCreator owner)
            : base(owner)
        {
            if (owner is CircularArrayCreator circle)
            {
                _circle = circle;
            }

            Debug.Assert(_circle != null, "Attempting to an invalid modifier");
        }

        public override void OnRemoved()
        {
            //
        }

        public override void Process(GameObject[] objs)
        {
            int numObjs = objs.Length;
            Vector3 center = _circle.Center;
            GameObject current = null;
            for (int i = 0; i < numObjs; ++i)
            {
                current = objs[i];
                Vector3 position = current.transform.position;
                Vector3 cross = Vector3.Cross((position - center).normalized, _circle.UpVector);
                current.transform.rotation = Quaternion.LookRotation(cross);
            }
        }

        protected override void OnInspectorUpdate()
        {
            //
        }
    }
}