using Prefabrikator.Runtime;
using Prefabrikator.Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class UniformModifier : Modifier
    {
        public Shared<Vector3> UniformValue => _uniformValue;
        [SerializeField] protected Shared<Vector3> _uniformValue = null;

        public Shared<float> ConstrainedValue => _constrainedValue;
        [SerializeField] protected Shared<float> _constrainedValue = null;

        public Shared<bool> ShouldConstrain => _constrainProportions;
        [SerializeField] protected Shared<bool> _constrainProportions = new();

        public UniformModifier(IRuntimeShape target, string label, float defaultConstainedValue)
        {
            _uniformValue = new Shared<Vector3>(new Vector3(defaultConstainedValue, defaultConstainedValue, defaultConstainedValue));
            
            //OnValueSetDelegate<Vector3> onValueChanged = (current, previous) => target.CommandQueue.Enqueue(new GenericCommand<Vector3>(_target, previous, current));
            //    _targetProperty = new Vector3Property(label, _target, onValueChanged);

            //_constrainedValue = new Shared<float>(defaultConstainedValue);
            //_constrainProperty = new ToggleProperty(new GUIContent("Lock Axes"), _constrainProportions, CreateCommand(_constrainProportions, target));
            //_constrainedProperty = new FloatProperty(label, _constrainedValue, CreateCommand(_constrainedValue, target));
        }

        public override sealed TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            ApplyModifier(target, proxies);

            return proxies;
        }

        public sealed override void OnRemoved(IRuntimeShape target, Transform[] proxies)
        {
            Teardown(target, proxies);
        }

        public sealed override void Teardown(IRuntimeShape target, Transform[] proxies)
        {
            target.ApplyToAll(proxies, RestoreDefault);
        }

        protected abstract void RestoreDefault(IRuntimeShape target, Transform obj);
        protected abstract void ApplyModifier(IRuntimeShape target, TransformProxy[] proxies);
    }
}