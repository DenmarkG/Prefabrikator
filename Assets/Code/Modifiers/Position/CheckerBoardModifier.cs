using UnityEngine;

namespace Prefabrikator
{
    class CheckerBoardModifier : Modifier
    {
        protected override string DisplayName => throw new System.NotImplementedException();

        private Shared<Vector3> _offset = new Shared<Vector3>();

        public CheckerBoardModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Process(GameObject[] objs)
        {
            //
        }

        protected override void OnInspectorUpdate()
        {
            //
        }

        public override void Teardown()
        {
            Owner.ApplyToAll((go, index) => { go.transform.position = Owner.GetDefaultPositionAtIndex(index); });
        }
    }
}
