using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class UniformModifier : Modifier
    {
        protected TransformProxy[] _targets = null;
        protected Shared<Vector3> _target = null;
        protected Vector3Property _targetProperty = null;

        protected Shared<float> _constrainedValue = null;
        protected FloatProperty _constrainedProperty = null;

        protected Shared<bool> _constrainProportions = new();
        protected ToggleProperty _constrainProperty = null;

        public UniformModifier(ArrayCreator owner, string label, float defaultConstainedValue)
            : base(owner)
        {
            _target = new Shared<Vector3>(new Vector3(defaultConstainedValue, defaultConstainedValue, defaultConstainedValue));
            _targetProperty = new Vector3Property(label, _target, OnValueChanged);

            _constrainedValue = new Shared<float>(defaultConstainedValue);
            _constrainProperty = new ToggleProperty(new GUIContent("Lock Axes"), _constrainProportions, CreateCommand(_constrainProportions));
            _constrainedProperty = new FloatProperty(label, _constrainedValue, CreateCommand(_constrainedValue));
        }

        public override sealed TransformProxy[] Process(TransformProxy[] proxies)
        {
            if (_targets == null || proxies.Length != _targets.Length)
            {
                _targets = proxies;
            }

            ApplyModifier(proxies);

            return proxies;
        }

        public sealed override void OnRemoved()
        {
            Teardown();
        }

        public sealed override void Teardown()
        {
            Owner.ApplyToAll(RestoreDefault);
        }

        protected sealed override void OnInspectorUpdate()
        {
            _constrainProportions.Set(_constrainProperty.Update());

            if (_constrainProportions)
            {
                float scale = _constrainedProperty.Update();
                _target.Set(new Vector3(scale, scale, scale));
            }
            else
            {
                _target.Set(_targetProperty.Update());
            }
        }

        protected void OnValueChanged(Vector3 current, Vector3 previous)
        {
            Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_target, previous, current));
        }

        protected abstract void RestoreDefault(GameObject obj);
        protected abstract void ApplyModifier(TransformProxy[] proxies);
    }
}