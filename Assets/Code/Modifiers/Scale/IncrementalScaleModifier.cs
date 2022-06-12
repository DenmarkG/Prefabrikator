using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalScaleModifier : Modifier
    {
        protected override string DisplayName => "Incremental Scale";

        private Shared<Vector3> _targetScale = new Shared<Vector3>(new Vector3(2f, 2f, 2f));
        private Vector3Property _targetProperty = null;

        //private bool _reverseDirection = false;

        public IncrementalScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            void OnTargetChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_targetScale, previous, current));
            }
            _targetProperty = new Vector3Property("Target Rotation", _targetScale, OnTargetChanged);
        }
        
        public override void Process(GameObject[] objs)
        {
            // #DG: make this account for change to starting scale
            Vector3 defaultScale = Owner.GetDefaultScale();
            int numObjs = objs.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Vector3 scale = Vector3.Lerp(defaultScale, _targetScale, t);
                objs[i].transform.localScale = scale;
            }
        }

        public override void OnRemoved()
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            Owner.ApplyToAll((go) => { go.transform.localScale = defaultScale; });
        }

        protected override void OnInspectorUpdate()
        {
            _targetScale.Set(_targetProperty.Update());
        }
    }
}
