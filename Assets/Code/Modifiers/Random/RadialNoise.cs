using System;
using UnityEngine;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
    public class RadialNoise : RandomModifier<float>
    {
        protected override string DisplayName => ModifierType.RadialNoise;

        private IRadial _radialShape = null;
        private float[] _radii = null;

        private static readonly float DefaultMin = .5f;
        private static readonly float DefaultMax = 3f;

        private FloatProperty _minProperty = null;
        private FloatProperty _maxProperty = null;

        public RadialNoise(ArrayCreator owner)
            : base(owner) 
        {
            _radialShape = owner as IRadial;
            Debug.Assert(_radialShape != null, "Not a radial Shape. Cannot add radial noise");

            _radii = new float[Owner.CreatedObjects.Count];

            float radius = _radialShape.Radius;
            _min.Set(radius - DefaultMin);
            _max.Set(radius + DefaultMax);

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

            GameObject current = null;
            int count = objs.Length;
            for (int i = 0; i < count; ++i)
            {
                current = objs[i];
                Vector3 direction = current.transform.position - center;
                direction.Normalize();
                direction *= _radii[i];
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
            for (int i = startingIndex; i < _radii.Length; ++i)
            {
                _radii[i] = RNG.Range(_min, _max);
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
