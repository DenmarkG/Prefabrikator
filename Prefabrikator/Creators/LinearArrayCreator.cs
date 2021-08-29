using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class LinearArrayData : ArrayData
    {
        public Vector3 Offset;

        public LinearArrayData(GameObject prefab, Vector3 targetScale, Quaternion targetRotation)
            : base(ArrayType.Line, prefab, targetScale, targetRotation)
        {
            //
        }
    }

    // #DG: TODO - Add Bidirectional option
    public class LinearArrayCreator : ArrayCreator
    {
        public override float MaxWindowHeight => 300f;
        public override string Name => "Line";
        private Vector3 _offset = new Vector3(2, 0, 0);

        public LinearArrayCreator(GameObject target)
            : base(target)
        {
            //
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
                {
                    EditorGUILayout.LabelField("Offset", GUILayout.Width(ArrayToolExtensions.LabelWidth));
                    Vector3 offset = EditorGUILayout.Vector3Field(string.Empty, _offset, null);
                    if (offset != _offset)
                    {
                        _offset = offset;
                        _needsRefresh = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (ArrayToolExtensions.DisplayCountField(ref _targetCount))
                {
                    _needsRefresh = true;
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (_needsRefresh)
                {
                    Refresh();
                }

                // Update Counts
                if (_createdObjects.Count != _targetCount)
                {
                    OnCountChange();
                }

                // Update positions
                OnOffsetChange();
            }
        }

        public override void Refresh(bool hardRefresh = false, bool useDefaultData = false)
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
            UpdateLocalScales();
            UpdateLocalRotations();

            _needsRefresh = false;
        }

        private void UpdatePositions()
        {
            if (_createdObjects.Count > 0)
            {
                GameObject currentObj = null;

                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    Vector3 offset = _offset * i;

                    currentObj = _createdObjects[i];
                    currentObj.transform.position = _targetProxy.transform.position + offset;
                }
            }
        }

        private void OnOffsetChange()
        {
            UpdatePositions();
        }

        private void OnCountChange()
        {
            // #DG: Turn this into command Queue. Currently there is a bug where this is also called when calling undo which is wrong
            ExecuteCommand(new CountChangeCommand(this, _createdObjects.Count, _targetCount));
            Refresh();
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
            LinearArrayData data = new LinearArrayData(_target, _targetScale, _targetRotation);
            data.Count = _targetCount;
            data.Offset = _offset;
            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is LinearArrayData lineData)
            {
                _targetCount = lineData.Count;
                _offset = lineData.Offset;
                _targetScale = lineData.TargetScale;
                _targetRotation = lineData.TargetRotation;
            }
        }
    }
}
