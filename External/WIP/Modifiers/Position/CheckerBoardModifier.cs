using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using System.Collections.Generic;
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

        public override void OnRemoved(IRuntimeShape target, Transform[] proxies)
        {
            Teardown(target, proxies);
        }

        public override TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            return proxies;
        }

        public override void Teardown(IRuntimeShape target, Transform[] proxies)
        {
            // #DG: Instead, apply the inverse of the transform of this modifier
            //target.ApplyToAll(proxies, (shape, proxy, index) => { proxy.position = target.GetDefaultPositionAtIndex(index); });
        }
    }
}
