using Prefabrikator.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class PositionNoiseModifier : Modifier
    {
        public override string DisplayName => "Position Noise";

        [SerializeField] private Vector3[] _positions = null;

        [SerializeField] private static readonly float DefaultMin = -.5f;
        [SerializeField] private static readonly float DefaultMax = .5f;

        [SerializeField] private Shared<Vector3> _minVector = new Shared<Vector3>(new Vector3(DefaultMin, DefaultMin, DefaultMin));
        [SerializeField] private Shared<Vector3> _maxVector = new Shared<Vector3>(new Vector3(DefaultMax, DefaultMax, DefaultMax));

        //[SerializeField] private Vector3Property _minProperty = null;
        //[SerializeField] private Vector3Property _maxProperty = null;

        public PositionNoiseModifier(IRuntimeShape target)
        {
            //SetupProperties(target);

            //int numObjs = target.Clones.Count;
            //_positions = new Vector3[numObjs];
            //for (int i = 0; i < numObjs; ++i)
            //{
            //    _positions[i] = new Vector3(1f, 1f, 1f);
            //}

            Randomize(target);
        }

        public override void OnRemoved(IRuntimeShape target, Transform[] proxies)
        {
            Teardown(target, proxies);
        }

        public override void Teardown(IRuntimeShape target, Transform[] proxies)
        {
            //target.ApplyToAll((_, go, index) => { go.transform.position = target.GetDefaultPositionAtIndex(index); });
        }

        public override TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            UpdateArray(target, proxies);

            int numObjs = proxies.Length;
            Vector3 position = Vector3.zero;
            for (int i = 0; i < numObjs; ++i)
            {
                // #DG: Need to store default positions
                position = Extensions.BiUnitLerp(_minVector, _maxVector, _positions[i]);
                proxies[i].Position = position + proxies[i].Position;
            }

            return proxies;
        }

        //protected override void OnInspectorUpdate(IRuntimeShape target, Transform[] proxies)
        //{
        //    _minVector.Set(_minProperty.Update());
        //    _maxVector.Set(_maxProperty.Update());

        //    if (GUILayout.Button("Randomize"))
        //    {
        //        Randomize(target);
        //    }
        //}

        //private void SetupProperties(IRuntimeShape target)
        //{
        //    void OnMinVectorChanged(Vector3 current, Vector3 previous)
        //    {
        //        target.CommandQueue.Enqueue(new GenericCommand<Vector3>(_minVector, previous, current));
        //    }
        //    _minProperty = new Vector3Property("Min", _minVector, OnMinVectorChanged);

        //    void OnMaxVectorChanged(Vector3 current, Vector3 previous)
        //    {
        //        target.CommandQueue.Enqueue(new GenericCommand<Vector3>(_maxVector, previous, current));
        //    }
        //    _maxProperty = new Vector3Property("Max", _maxVector, OnMaxVectorChanged);
        //}

        private void Randomize(IRuntimeShape target, int startingIndex = 0)
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
                //    var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _positions, ApplyPositions);
                //    target.CommandQueue.Enqueue(valueChanged);
            }
        }

        // #DG: fix this
        private void UpdateArray(IRuntimeShape target, TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;

            if (_positions == null)
            {
                _positions = new Vector3[numObjs];
                for (int i = 0; i < numObjs; ++i)
                {
                    _positions[i] = new Vector3(1f, 1f, 1f);
                }

                Randomize(target    );
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
                    Randomize(target, startingIndex);
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
