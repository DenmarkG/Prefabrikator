using Prefabrikator.Runtime;
using UnityEngine;

namespace Prefabrikator
{
    class CheckerBoardModifier : Modifier
    {
        public override string DisplayName => throw new System.NotImplementedException();

        private Shared<Vector3> _offset = new Shared<Vector3>();

        public CheckerBoardModifier(IRuntimeShape owner)
        {
            //
        }

        public override void OnRemoved(IRuntimeShape target)
        {
            Teardown(target);
        }

        public override TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            return proxies;
        }

        protected override void OnInspectorUpdate(IRuntimeShape target)
        {
            //
        }

        public override void Teardown(IRuntimeShape target)
        {
            target.ApplyToAll((shape, go, index) => { go.transform.position = target.GetDefaultPositionAtIndex(index); });
        }
    }
}
