using UnityEngine;

namespace Prefabrikator
{
    class CheckerBoardModifier : Modifier
    {
        protected override string DisplayName => throw new System.NotImplementedException();

        private Shared<Vector3> _offset = new Shared<Vector3>();
        private Vector3Property _offsetProperty = null;

        public CheckerBoardModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }

        public override void OnRemoved()
        {
            Owner.ApplyToAll((go, index) => { go.transform.position = Owner.GetDefaultPositionAtIndex(index); });
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
