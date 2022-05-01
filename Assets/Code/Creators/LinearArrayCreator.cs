using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class LinearArrayData : ArrayData
    {
        public Vector3 Offset;

        public LinearArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ArrayType.Line, prefab, targetRotation)
        {
            //
        }
    }

    public class LinearArrayCreator : ArrayCreator
    {
        public static readonly int MinCount = 2;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Line";
        private Shared<Vector3> _offset = new Shared<Vector3>(new Vector3(2f, 0f, 0f));

        private Vector3Property _offsetProperty = null;

        public LinearArrayCreator(GameObject target)
            : base(target)
        {
            _targetCount = MinCount;

            void OnValueSet(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_offset, previous, current));
            };

            _offsetProperty = new Vector3Property("Offset", _offset, OnValueSet);

            Refresh();
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
                {
                    //_offset = _offsetProperty.Update();
                    _offset.Set(_offsetProperty.Update());
                }
                EditorGUILayout.EndHorizontal();

                int currentTargetCount = _targetCount;
                if (Extensions.DisplayCountField(ref currentTargetCount))
                {
                    CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, currentTargetCount));
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (NeedsRefresh)
                {
                    Refresh();
                }

                // Update positions
                OnOffsetChange();
            }
        }

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
            }

            EstablishHelper(useDefaultData);

            if (_targetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > _targetCount)
                {
                    int index = _createdObjects.Count - 1;
                    if (index >= 0)
                    {
                        DestroyClone(_createdObjects[_createdObjects.Count - 1]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                while (_targetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }

            UpdatePositions();
            UpdateLocalRotations();
        }

        private void UpdatePositions()
        {

            if (_createdObjects.Count > 0)
            {
                Undo.RecordObjects(_createdObjects.ToArray(), "Changed offset");
                GameObject currentObj = null;

                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    Vector3 offset = (Vector3)_offset * i;

                    currentObj = _createdObjects[i];
                    currentObj.transform.position = _targetProxy.transform.position + offset;
                }
            }
        }

        private void OnOffsetChange()
        {
            UpdatePositions();
        }

        private void CreateClone()
        {
            GameObject clone = GameObject.Instantiate(_target, _target.transform.position, _target.transform.rotation, _target.transform.parent);
            clone.transform.SetParent(_targetProxy.transform);

            int lastIndex = _createdObjects.Count - 1;

            if (_createdObjects.Count > 0)
            {
                clone.transform.position = _createdObjects[lastIndex].transform.position + _offset;
                clone.transform.rotation = _createdObjects[lastIndex].transform.rotation;
            }
            else
            {
                clone.transform.position = _target.transform.position + _offset;
                clone.transform.rotation = _target.transform.rotation;
            }

            _createdObjects.Add(clone);
        }

        protected override ArrayData GetContainerData()
        {
            LinearArrayData data = new LinearArrayData(_target, _targetRotation);
            data.Count = _targetCount;
            data.Offset = _offset;
            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is LinearArrayData lineData)
            {
                _targetCount = lineData.Count;
                _offset.Set(lineData.Offset);
                _targetRotation = lineData.TargetRotation;
            }
        }
    }
}
