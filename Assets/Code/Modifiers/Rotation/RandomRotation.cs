using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class RandomRotation : Modifier
    {
        protected override string DisplayName => "Random Rotation";

        private Shared<Vector3> _min = new Shared<Vector3>();
        private Shared<Vector3> _max = new Shared<Vector3>();

        private Vector3Property _minProperty = null;
        private Vector3Property _maxProperty = null;

        private Vector3[] _rotations = null;

        public RandomRotation(ArrayCreator owner)
            : base(owner)
        {
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
            Quaternion defaultRotation = Owner.GetDefaultRotation();
            Owner.ApplyToAll((go) => { go.transform.rotation = defaultRotation; });
        }

        public override void Process(GameObject[] objs)
        {
            UpdateArray(objs);
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

        private void Randomize(int startingIndex = 0)
        {
            int numObjs = _rotations.Length;
            Vector3[] previousValues = new Vector3[_rotations.Length];

            _rotations = new Vector3[numObjs];
            for (int i = 0; i < numObjs; ++i)
            {
                previousValues[i] = _rotations[i];
                _rotations[i] = Random.insideUnitSphere;
            }

            void ApplyRotations(Vector3[] rotationsToApply)
            {
                _rotations = rotationsToApply;
            }

            var valueChanged = new ValueChangedCommand<Vector3[]>(previousValues, _rotations, ApplyRotations);
            Owner.CommandQueue.Enqueue(valueChanged);
        }

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

            for (int i = 0; i < numObjs; ++i)
            {
                Vector3 rot = _rotations[i];

                rot.x = Mathf.LerpUnclamped(_min.Get().x, _max.Get().x, rot.x);
                rot.y = Mathf.LerpUnclamped(_min.Get().y, _max.Get().y, rot.y);
                rot.z = Mathf.LerpUnclamped(_min.Get().z, _max.Get().z, rot.z);

                Vector3 vect = Extensions.Clamp(rot, _min, _max);

                objs[i].transform.rotation = Quaternion.Euler(vect);
            }
        }
    }
}
