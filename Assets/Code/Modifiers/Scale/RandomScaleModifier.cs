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
        
        private Shared<Vector3> _min = new Shared<Vector3>(new Vector3(.5f, .5f, .5f));
        private Shared<Vector3> _max = new Shared<Vector3>(new Vector3(3f, 3f, 3f));

        private Vector3Property _minProperty = null;
        private Vector3Property _maxProperty = null;

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
            for (int i = 0; i < numObjs; ++i)
            {
                Vector3 scale = _scales[i];

                scale.x = Mathf.LerpUnclamped(_min.Get().x, _max.Get().x, scale.x);
                scale.y = Mathf.LerpUnclamped(_min.Get().y, _max.Get().y, scale.y);
                scale.z = Mathf.LerpUnclamped(_min.Get().z, _max.Get().z, scale.z);

                objs[i].transform.localScale = Extensions.Clamp(scale, _min, _max); ;
            }
        }

        public override void OnRemoved()
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            Owner.ApplyToAll((go) => { go.transform.localScale = defaultScale; });
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

            var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _scales, ApplyScales);
            Owner.CommandQueue.Enqueue(valueChanged);           
        }

        private void SetupProperties()
        {
            void OnMinChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_min, previous, current));
            }
            _minProperty = new Vector3Property("Min", _min, OnMinChanged);

            void OnMaxChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_max, previous, current));
            }
            _maxProperty = new Vector3Property("Max", _max, OnMaxChanged);
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
