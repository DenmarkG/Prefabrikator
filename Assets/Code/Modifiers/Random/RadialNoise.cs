using Prefabrikator.Runtime;
using System;
using UnityEngine;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
    using Shapes;

    public class RadialNoise : RandomModifier<float>
    {
        public override string DisplayName => ModifierType.RadialNoise;

        [SerializeField] private IRadial _radialShape = null;
        [SerializeField] private float[] _radialDelta = null;

        [SerializeField] private static readonly float DefaultMin = -1.5f;
        [SerializeField] private static readonly float DefaultMax = 1.5f;

        [SerializeField] private FloatProperty _minProperty = null;
        [SerializeField] private FloatProperty _maxProperty = null;

        public RadialNoise(IShape target)
        {
            _radialShape = target as IRadial;
            Debug.Assert(_radialShape != null, "Not a radial Shape. Cannot add radial noise");

            _radialDelta = new float[target.Clones.Count];

            float radius = _radialShape.Radius;
            _min.Set(DefaultMin);
            _max.Set(DefaultMax);

            SetupProperties(target);
            Randomize(target);
        }

        public override void OnRemoved(IRuntimeShape target, Transform[] proxies)
        {
            Teardown(target, proxies);
        }

        public override Transform[] Process(IRuntimeShape target, Transform[] proxies)
        {
            Vector3 center = _radialShape.Center;
            float radius = _radialShape.Radius;

            Transform current;
            int count = proxies.Length;
            for (int i = 0; i < count; ++i)
            {
                current = proxies[i];
                Vector3 direction = current.position - center;
                direction.Normalize();
                direction *= _radialDelta[i] + radius;
                proxies[i].position = center + direction;
            }

            return proxies;
        }

        public override void Teardown(IRuntimeShape target, Transform[] proxies)
        {
            target.ApplyToAll(proxies, (target, proxy, index) =>
            {
                go.transform.position = target.GetDefaultPositionAtIndex(index);
            });
        }

        private Quaternion GetInverseRotationAtIndex(int index)
        {
            return Quaternion.identity;
        }

        protected override void OnInspectorUpdate(IRuntimeShape target)
        {
            _min.Set(_minProperty.Update());
            _max.Set(_maxProperty.Update());

            if (GUILayout.Button("Randomize"))
            {
                Randomize(target);
            }
        }

        protected override void Randomize(IRuntimeShape target, int startingIndex = 0)
        {
            int numObjs = _radialDelta.Length;
            float[] previous = new float[numObjs];
            for (int i = startingIndex; i < numObjs; ++i)
            {
                previous[i] = _radialDelta[i];
                _radialDelta[i] = RNG.Range(_min, _max);
            }

            void ApplyScales(float[] deltas)
            {
                _radialDelta = deltas;
            }

            if (startingIndex == 0)
            {
                var valueChanged = new ValueChangedCommand<float[]>(previous, _radialDelta, ApplyScales);
                target.CommandQueue.Enqueue(valueChanged);
            }
        }

        private void SetupProperties(IShape target)
        {
            const string Min = "Min";
            const string Max = "Max";

            void OnMinChanged(float current, float previous)
            {
                target.CommandQueue.Enqueue(new GenericCommand<float>(_min, previous, current));
            }
            _minProperty = new FloatProperty(Min, _min, OnMinChanged);

            void OnMaxChanged(float current, float previous)
            {
                target.CommandQueue.Enqueue(new GenericCommand<float>(_max, previous, current));
            }
            _maxProperty = new FloatProperty(Max, _max, OnMaxChanged);
        }
    }
}
