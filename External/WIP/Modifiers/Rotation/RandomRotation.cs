using Prefabrikator.Runtime;
using UnityEngine;

namespace Prefabrikator
{
    public class RandomRotation : RandomModifier<Vector3>
    {
        public override string DisplayName => "Random Rotation";        
        [SerializeField] private Vector3[] _rotations = null;

        // #DG: move to editor only code and replace with member variables
        [SerializeField] private Vector3Property _minProperty = null;
        [SerializeField] private Vector3Property _maxProperty = null;

        public RandomRotation(IRuntimeShape target)
        {
            _min = new Shared<Vector3>(new Vector3(-179f, -179f, -179f));
            _max = new Shared<Vector3>(new Vector3(180f, 180f, 180f));

            int numObjs = target.Clones.Count;
            _rotations = new Vector3[numObjs];
            for (int i = 0; i < numObjs; ++i)
            {
                _rotations[i] = Random.insideUnitSphere;
            }

            SetupProperties(target);
        }

        public override void OnRemoved(IRuntimeShape target, Transform[] xforms)
        {
            Teardown(target, xforms);
        }

        public override void Teardown(IRuntimeShape target, Transform[] xforms)
        {
            Quaternion defaultRotation = target.GetDefaultRotation();
            target.ApplyToAll((_, go) => { go.transform.rotation = defaultRotation; });
        }

        public override TransformProxy[] Process(IRuntimeShape target, TransformProxy[] proxies)
        {
            UpdateArray(target, proxies);

            IRotator rotator = null;
            bool isAdditive = IsAdditive(target, out rotator);

            int numObjs = proxies.Length;
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

                proxies[i].Rotation = rotation;
            }

            return proxies;
        }

        protected override void OnInspectorUpdate(IRuntimeShape target, Transform[] xforms)
        {
            _min.Set(_minProperty.Update());
            _max.Set(_maxProperty.Update());

            if (GUILayout.Button("Randomize"))
            {
                Randomize(target);
            }
        }

        private void SetupProperties(IShape target)
        {
            void OnMinChanged(Vector3 current, Vector3 previous)
            {
                target.CommandQueue.Enqueue(new GenericCommand<Vector3>(_min, previous, current));
            }
            _minProperty = new Vector3Property("Min", _min, OnMinChanged);

            void OnMaxChanged(Vector3 current, Vector3 previous)
            {
                target.CommandQueue.Enqueue(new GenericCommand<Vector3>(_max, previous, current));
            }
            _maxProperty = new Vector3Property("Max", _max, OnMaxChanged);
        }

        protected override void Randomize(IRuntimeShape target, int startingIndex = 0)
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
                target.CommandQueue.Enqueue(valueChanged);
            }
        }

        // #DG: Move this to parent. 
        private void UpdateArray(IRuntimeShape target, TransformProxy[] proxies)
        {
            int numObjs = proxies.Length;

            if (_rotations == null)
            {
                _rotations = new Vector3[numObjs];
                for (int i = 0; i < numObjs; ++i)
                {
                    _rotations[i] = new Vector3(1f, 1f, 1f);
                }

                Randomize(target);
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
                    Randomize(target, startingIndex);
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

        private bool IsAdditive(IRuntimeShape target, out IRotator rotator)
        {
            int? index = target.GetIndexOfModifier(this);
            if (index != null)
            {
                rotator = target.GetUpstreamModifierOfType<IRotator>(index.Value);
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
