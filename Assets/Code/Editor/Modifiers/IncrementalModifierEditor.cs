using Prefabrikator.Runtime;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Prefabrikator.Editor
{
    public class IncrementalModifierEditor : ModifierEditor
    {
        [SerializeField] private Vector3Property _targetProperty = null;

        protected override void OnInspectorUpdate(Modifier modifier, IShape target, Transform[] proxies)
        {
            //void OnTargetChanged(Vector3 current, Vector3 previous)
            //{
            //    target.CommandQueue.Enqueue(new GenericCommand<Vector3>(Target, previous, current));
            //}
            //_targetProperty = new Vector3Property("Target", Target, OnTargetChanged);

            Target.Set(_targetProperty.Update());
        }
    }
}