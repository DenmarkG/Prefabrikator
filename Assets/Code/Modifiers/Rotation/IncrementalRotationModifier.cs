using System.Collections;
using UnityEngine;

namespace Prefabrikator
{
    public class IncrementalRotationModifier : Modifier
    {
        protected override string DisplayName => "Incremental Rotation";

        private Shared<Vector3> _targetRotation = new Shared<Vector3>(new Vector3(0f, 180f, 0f));
        private Vector3Property _targetProperty = null;

        private bool _reverseDirection = false;

        public IncrementalRotationModifier(ArrayCreator owner)
            : base(owner)
        {
            void OnTargetChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_targetRotation, previous, current));
            }
            _targetProperty = new Vector3Property("Target Rotation", _targetRotation, OnTargetChanged);
        }

        public override void Process(GameObject[] objs)
        {
            int numObjs = objs.Length;
            // #DG: make this account for changes to the starting rotation (uniform mod)
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            for (int i = 0; i < numObjs; ++i)
            {
                float t = (float)i / (numObjs - 1);
                Quaternion rotation = Quaternion.Lerp(defaultRotation, Quaternion.Euler(_targetRotation), t);
                objs[i].transform.rotation = rotation;
            }
        }

        public override void OnRemoved()
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }

        protected override void OnInspectorUpdate()
        {
            _targetRotation.Set(_targetProperty.Update());
        }
    }
}
