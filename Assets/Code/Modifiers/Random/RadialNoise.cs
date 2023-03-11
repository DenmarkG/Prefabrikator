using System;
using UnityEngine;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
    public class RadialNoise : RandomModifier<float>
    {
        protected override string DisplayName => ModifierType.RadialNoise;

        private IRadial _radialShape = null;
        private float[] _radialDelta = null;

        private static readonly float DefaultMin = -1.5f;
        private static readonly float DefaultMax = 1.5f;

        private FloatProperty _minProperty = null;
        private FloatProperty _maxProperty = null;

        public RadialNoise(ArrayCreator owner)
            : base(owner) 
        {
            _radialShape = owner as IRadial;
            Debug.Assert(_radialShape != null, "Not a radial Shape. Cannot add radial noise");

            _radialDelta = new float[Owner.CreatedObjects.Count];

            float radius = _radialShape.Radius;
            _min.Set(DefaultMin);
            _max.Set(DefaultMax);

            SetupProperties();
            Randomize();
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Process(GameObject[] objs)
        {
            Vector3 center = _radialShape.Center;
            float radius = _radialShape.Radius;

            GameObject current = null;
            int count = objs.Length;
            for (int i = 0; i < count; ++i)
            {
                current = objs[i];
                Vector3 direction = current.transform.position - center;
                direction.Normalize();
                direction *= _radialDelta[i] + radius;
                current.transform.position = center + direction;
            }
        }

        public override void Teardown()
        {
            Owner.ApplyToAll((go, index) =>
            {
                go.transform.position = Owner.GetDefaultPositionAtIndex(index);
            });
        }

        protected override void OnInspectorUpdate()
        {
            _min.Set(_minProperty.Update());
            _max.Set(_maxProperty.Update());

            if (GUILayout.Button("Randomize"))
            {
                Randomize();
            }
        }

        protected override void Randomize(int startingIndex = 0)
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
                Owner.CommandQueue.Enqueue(valueChanged);
            }
        }

        private void SetupProperties()
        {
            const string Min = "Min";
            const string Max = "Max";

            void OnMinChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_min, previous, current));
            }
            _minProperty = new FloatProperty(Min, _min, OnMinChanged);

            void OnMaxChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_max, previous, current));
            }
            _maxProperty = new FloatProperty(Max, _max, OnMaxChanged);
        }
    }
}
