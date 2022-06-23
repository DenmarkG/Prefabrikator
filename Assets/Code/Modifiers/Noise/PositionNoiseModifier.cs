using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class PositionNoiseModifier : Modifier
    {
        protected override string DisplayName => "Position Noise";

        private Vector3[] _positions = null;

        private static readonly float DefaultMin = -.5f;
        private static readonly float DefaultMax = .5f;

        private Shared<Vector3> _minVector = new Shared<Vector3>(new Vector3(DefaultMin, DefaultMin, DefaultMin));
        private Shared<Vector3> _maxVector = new Shared<Vector3>(new Vector3(DefaultMax, DefaultMax, DefaultMax));

        private Vector3Property _minProperty = null;
        private Vector3Property _maxProperty = null;

        public PositionNoiseModifier(ArrayCreator owner)
            : base(owner)
        {
            SetupProperties();

            int numObjs = Owner.CreatedObjects.Count;
            _positions = new Vector3[numObjs];
            for (int i = 0; i < numObjs; ++i)
            {
                _positions[i] = new Vector3(1f, 1f, 1f);
            }

            Randomize();
        }

        public override void OnRemoved()
        {
            Owner.ApplyToAll((go, index) => { go.transform.position -= _positions[index]; });
        }

        public override void Process(GameObject[] objs)
        {
            UpdateArray(objs);

            int numObjs = objs.Length;
            Vector3 position = Vector3.zero;
            for (int i = 0; i < numObjs; ++i)
            {
                // #DG: Need to store default positions
                position = Extensions.BiUnitLerp(_minVector, _maxVector, _positions[i]);
                objs[i].transform.position = position + Owner.GetDefaultPositionAtIndex(i);
            }
        }

        protected override void OnInspectorUpdate()
        {
            _minVector.Set(_minProperty.Update());
            _maxVector.Set(_maxProperty.Update());
        }

        private void SetupProperties()
        {
            void OnMinVectorChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_minVector, previous, current));
            }
            _minProperty = new Vector3Property("Min", _minVector, OnMinVectorChanged);

            void OnMaxVectorChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_maxVector, previous, current));
            }
            _maxProperty = new Vector3Property("Max", _maxVector, OnMaxVectorChanged);
        }

        private void Randomize(int startingIndex = 0)
        {
            int numObjs = _positions.Length;
            Vector3[] previousValues = new Vector3[_positions.Length];

            for (int i = startingIndex; i < numObjs; ++i)
            {
                previousValues[i] = _positions[i];
                _positions[i] = Random.insideUnitSphere;
            }

            void ApplyPositions(Vector3[] positionsToApply)
            {
                _positions = positionsToApply;
            }

            if (startingIndex == 0)
            {
                var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _positions, ApplyPositions);
                Owner.CommandQueue.Enqueue(valueChanged);
            }
        }

        private void UpdateArray(GameObject[] objs)
        {
            int numObjs = objs.Length;

            if (_positions == null)
            {
                _positions = new Vector3[numObjs];
                for (int i = 0; i < numObjs; ++i)
                {
                    _positions[i] = new Vector3(1f, 1f, 1f);
                }

                Randomize();
            }
            else if (numObjs != _positions.Length)
            {
                // #DG: This breaks undo
                Vector3[] temp = new Vector3[numObjs];
                int startingIndex = 0;
                if (_positions.Length < numObjs)
                {
                    startingIndex = _positions.Length;
                    _positions.CopyTo(temp, 0);
                    _positions = temp;
                    Randomize(startingIndex);
                }
                else if (_positions.Length > numObjs)
                {
                    for (int i = 0; i < numObjs; ++i)
                    {
                        temp[i] = _positions[i];
                    }

                    _positions = temp;
                }
            }
        }
    }
}
