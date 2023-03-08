using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class RandomRotation : RandomModifier
    {
        protected override string DisplayName => "Random Rotation";        
        private Vector3[] _rotations = null;

        public RandomRotation(ArrayCreator owner)
            : base(owner)
        {
            _min = new Shared<Vector3>(new Vector3(-179f, -179f, -179f));
            _max = new Shared<Vector3>(new Vector3(180f, 180f, 180f));

            int numObjs = Owner.CreatedObjects.Count;
            _rotations = new Vector3[numObjs];
            for (int i = 0; i < numObjs; ++i)
            {
                _rotations[i] = Random.insideUnitSphere;
            }

            SetupProperties();
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Teardown()
        {
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }

        public override void Process(GameObject[] objs)
        {
            UpdateArray(objs);

            IRotator rotator = null;
            bool isAdditive = IsAdditive(out rotator);

            int numObjs = objs.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                Vector3 rot = Extensions.BiUnitLerp(_min, _max, _rotations[i]);

                Quaternion rotation = Quaternion.identity;
                if (isAdditive)
                {
                    Quaternion defaultRotation = rotator.GetRotationAtIndex(i);
                    rotation = defaultRotation * Quaternion.Euler(Extensions.Clamp(rot, _min, _max));
                }
                else
                {
                    rotation = Quaternion.Euler(Extensions.Clamp(rot, _min, _max));
                }

                objs[i].transform.rotation = rotation;
            }
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

        protected override void Randomize(int startingIndex = 0)
        {
            int numObjs = _rotations.Length;
            Vector3[] previousValues = new Vector3[_rotations.Length];

            for (int i = 0; i < numObjs; ++i)
            {
                previousValues[i] = _rotations[i];
                _rotations[i] = Random.insideUnitSphere;
            }

            void ApplyRotations(Vector3[] rotationsToApply)
            {
                _rotations = rotationsToApply;
            }

            if (startingIndex == 0)
            {
                var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _rotations, ApplyRotations);
                Owner.CommandQueue.Enqueue(valueChanged);
            }
        }

        // #DG: Move this to parent. 
        private void UpdateArray(GameObject[] objs)
        {
            int numObjs = objs.Length;

            if (_rotations == null)
            {
                _rotations = new Vector3[numObjs];
                for (int i = 0; i < numObjs; ++i)
                {
                    _rotations[i] = new Vector3(1f, 1f, 1f);
                }

                Randomize();
            }
            else if (numObjs != _rotations.Length)
            {
                // #DG: This breaks undo
                Vector3[] temp = new Vector3[numObjs];
                int startingIndex = 0;
                if (_rotations.Length < numObjs)
                {
                    startingIndex = _rotations.Length;
                    _rotations.CopyTo(temp, 0);
                    _rotations = temp;
                    Randomize(startingIndex);
                }
                else if (_rotations.Length > numObjs)
                {
                    for (int i = 0; i < numObjs; ++i)
                    {
                        temp[i] = _rotations[i];
                    }

                    _rotations = temp;
                }
            }
        }

        private bool IsAdditive(out IRotator rotator)
        {
            int? index = Owner.GetIndexOfModifier(this);
            if (index != null)
            {
                rotator = Owner.GetUpstreamModifierOfType<IRotator>(index.Value);
                if (rotator != null)
                {
                    return true;
                }
            }

            rotator = null;
            return false;
        }
    }
}
