using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    // #DG: store the scales as normalized vectors that can be visualized as the range changes
    // Also store max and min as two vectors for simplicity
    public class RandomScaleModifier : Modifier
    {
        protected override string DisplayName => "Random Scale";

        private Vector3[] _scales = null;

        private static readonly float DefaultMin = .5f;
        private static readonly float DefaultMax = 3f;

        private Shared<Vector3> _minVector = new Shared<Vector3>(new Vector3(DefaultMin, DefaultMin, DefaultMin));
        private Shared<Vector3> _maxVector = new Shared<Vector3>(new Vector3(DefaultMax, DefaultMax, DefaultMax));

        private Vector3Property _minVectorProperty = null;
        private Vector3Property _maxVectorProperty = null;

        private Shared<float> _minFloat = new Shared<float>(DefaultMin);
        private Shared<float> _maxFloat = new Shared<float>(DefaultMax);

        private FloatProperty _minFloatProperty = null;
        private FloatProperty _maxFloatProperty = null;

        private Shared<bool> _keepAspectRatio = new Shared<bool>(false);
        private BoolProperty _keepAspectProperty = null;

        public RandomScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            SetupProperties();

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
            UpdateArray(objs);

            int numObjs = objs.Length;
            Vector3 scale = Vector3.one;
            for (int i = 0; i < numObjs; ++i)
            {
                if (_keepAspectRatio)
                {
                    scale = Owner.GetDefaultScale() * Mathf.Lerp(_minFloat, _maxFloat, _scales[i].x);
                }
                else
                {
                    scale = Extensions.BiUnitLerp(_minVector, _maxVector, _scales[i]);
                }
                
                objs[i].transform.localScale = scale;
            }
        }

        public override void OnRemoved()
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            Owner.ApplyToAll((go) => { go.transform.localScale = defaultScale; });
        }

        protected override void OnInspectorUpdate()
        {
            _keepAspectRatio.Set(_keepAspectProperty.Update());

            if (_keepAspectRatio)
            {
                _minFloat.Set(_minFloatProperty.Update());
                _minVector.Set(new Vector3(_minFloat, _minFloat, _minFloat));

                _maxFloat.Set(_maxFloatProperty.Update());
                _maxVector.Set(new Vector3(_maxFloat, _maxFloat, _maxFloat));
            }
            else
            {
                _minVector.Set(_minVectorProperty.Update());
                _maxVector.Set(_maxVectorProperty.Update());
            }

            if (GUILayout.Button("Randomize"))
            {
                Randomize();
            }
        }

        private void Randomize(int startingIndex = 0)
        {
            int numObjs = _scales.Length;
            Vector3[] previousValues = new Vector3[_scales.Length];

            for (int i = startingIndex; i < numObjs; ++i)
            {
                previousValues[i] = _scales[i];
                _scales[i] = Random.insideUnitSphere;
            }

            void ApplyScales(Vector3[] scalesToApply)
            {
                _scales = scalesToApply;
            }

            if (startingIndex == 0)
            {
                var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _scales, ApplyScales);
                Owner.CommandQueue.Enqueue(valueChanged);
            }
        }

        private void SetupProperties()
        {
            const string Min = "Min";
            const string Max = "Max";
            void OnMinVectorChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_minVector, previous, current));
            }
            _minVectorProperty = new Vector3Property(Min, _minVector, OnMinVectorChanged);

            void OnMaxVectorChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_maxVector, previous, current));
            }
            _maxVectorProperty = new Vector3Property(Max, _maxVector, OnMaxVectorChanged);

            void OnMinFloatChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_minFloat, previous, current));
            }
            _minFloatProperty = new FloatProperty(Min, _minFloat, OnMinFloatChanged);

            void OnMaxFloatChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_maxFloat, previous, current));
            }
            _maxFloatProperty = new FloatProperty(Max, _maxFloat, OnMaxFloatChanged);

            void OnKeepAspectChanged(bool current, bool previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<bool>(_keepAspectRatio, previous, current));
            }
            _keepAspectProperty = new BoolProperty("Keep Aspect Ratio", _keepAspectRatio, OnKeepAspectChanged);

        }

        private void UpdateArray(GameObject[] objs)
        {
            int numObjs = objs.Length;

            if (_scales == null)
            {
                _scales = new Vector3[numObjs];
                for (int i = 0; i < numObjs; ++i)
                {
                    _scales[i] = new Vector3(1f, 1f, 1f);
                }

                Randomize();
            }
            else if (numObjs != _scales.Length)
            {
                // #DG: This breaks undo
                Vector3[] temp = new Vector3[numObjs];
                int startingIndex = 0;
                if (_scales.Length < numObjs)
                {
                    startingIndex = _scales.Length;
                    _scales.CopyTo(temp, 0);
                    _scales = temp;
                    Randomize(startingIndex);
                }
                else if (_scales.Length > numObjs)
                {
                    for (int i = 0; i < numObjs; ++i)
                    {
                        temp[i] = _scales[i];
                    }

                    _scales = temp;
                }
            }
        }
    }
}
