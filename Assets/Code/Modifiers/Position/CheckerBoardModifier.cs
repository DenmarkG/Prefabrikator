using UnityEngine;

namespace Prefabrikator
{
    class CheckerBoardModifier : Modifier
    {
        public override string DisplayName => throw new System.NotImplementedException();

        private Shared<Vector3> _offset = new Shared<Vector3>();

        public CheckerBoardModifier(IShape owner)
        {
            //
        }

        public override void OnRemoved(IShape target)
        {
            Teardown(target);
        }

        public override TransformProxy[] Process(IShape target, TransformProxy[] proxies)
        {
            return proxies;
        }

        protected override void OnInspectorUpdate(IShape target)
        {
            //
        }

        public override void Teardown(IShape target)
        {
            target.ApplyToAll((shape, go, index) => { go.transform.position = target.GetDefaultPositionAtIndex(index); });
        }
    }
}
