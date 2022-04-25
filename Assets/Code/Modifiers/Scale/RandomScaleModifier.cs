using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class RandomScaleModifier : Modifier
    {
        protected override string DisplayName => "Random Scale";

        private Vector3[] _scales = null;
        private MinMax _xRange = null;
        private MinMaxProperty _xRangeProperty = null;
        private MinMaxProperty _yRangeProperty = null;
        private MinMaxProperty _zRangeProperty = null;

        private MinMax _yRange = null;
        private MinMax _zRange = null;

        public RandomScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            SetupRangeProperties();

            int numObjs = Owner.CreatedObjects.Count;
            _scales = new Vector3[numObjs];
            for (int i = 0; i < numObjs; ++i)
            {
                _scales[i] = new Vector3(1f, 1f, 1f);
            }

            Randomize();
        }

        public override void Process(GameObject[] objs)
        {
            int numObjs = objs.Length;

            if (_scales == null || numObjs != _scales.Length)
            {
                _scales = new Vector3[numObjs];
                for (int i = 0; i < numObjs; ++i)
                {
                    _scales[i] = new Vector3(1f, 1f, 1f);
                }

                Randomize();
            }

            for (int i = 0; i < numObjs; ++i)
            {
                objs[i].transform.localScale = _scales[i];
            }
        }

        protected override void OnInspectorUpdate()
        {
            _xRangeProperty.Update();

            if (GUILayout.Button("Randomize"))
            {
                Randomize();
            }
        }

        private void Randomize()
        {
            int numOjbs = _scales.Length;
            Vector3[] previousValues = new Vector3[_scales.Length];

            for (int i = 0; i < numOjbs; ++i)
            {
                previousValues[i] = _scales[i];
                Extensions.Randomize(ref _scales[i], _xRange, _yRange, _zRange);
            }

            void ApplyScales(Vector3[] scalesToApply)
            {
                _scales = scalesToApply;
            }

            var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _scales, ApplyScales);
            Owner.CommandQueue.Enqueue(valueChanged);            
        }

        private void SetupRangeProperties()
        {
            // X
            _xRange = new MinMax(0, 5);
            void OnXChanged(MinMax current, MinMax previous)
            {
                var valueCommand = new ValueChangedCommand<MinMax>(previous, current, (x) =>
                {
                    _xRange.Min = x.Min;
                    _xRange.Max = x.Max;
                });
                Owner.CommandQueue.Enqueue(valueCommand);
            };
            _xRangeProperty = new MinMaxProperty("X Range", _xRange, OnXChanged);

            // Y 
            _yRange = new MinMax(0, 5);
            void OnYChanged(MinMax current, MinMax previous)
            {
                var valueCommand = new ValueChangedCommand<MinMax>(previous, current, (y) =>
                {
                    _yRange.Min = y.Min;
                    _yRange.Max = y.Max;
                });
                Owner.CommandQueue.Enqueue(valueCommand);
            };
            _yRangeProperty = new MinMaxProperty("X Range", _xRange, OnYChanged);

            // Z
            _zRange = new MinMax(0, 5);
            void OnZChanged(MinMax current, MinMax previous)
            {
                var valueCommand = new ValueChangedCommand<MinMax>(previous, current, (z) =>
                {
                    _zRange.Min = z.Min;
                    _zRange.Max = z.Max;
                });
                Owner.CommandQueue.Enqueue(valueCommand);
            };
            _zRangeProperty = new MinMaxProperty("X Range", _xRange, OnZChanged);
        }
    }
}
