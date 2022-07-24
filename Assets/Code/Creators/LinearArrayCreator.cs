using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class LinearArrayData : ArrayData
    {
        public Vector3 Offset;

        public LinearArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Line, prefab, targetRotation)
        {
            //
        }
    }

    public class LinearArrayCreator : ArrayCreator
    {
        public override int MinCount => DefaultCount;
        public static readonly int DefaultCount = 2;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Line";
        private Shared<Vector3> _offset = new Shared<Vector3>(new Vector3(2f, 0f, 0f));

        private Vector3Property _offsetProperty = null;

        public LinearArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
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
                EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                {
                    _offset.Set(_offsetProperty.Update());
                }
                EditorGUILayout.EndHorizontal();

                ShowCountField();
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

            if (TargetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }

            UpdatePositions();
            UpdateLocalRotations(); // #DG: Remove this from the project
        }

        private void UpdatePositions()
        {
            GameObject proxy = GetProxy();

            if (_createdObjects.Count > 0 && proxy != null)
            {
                Undo.RecordObjects(_createdObjects.ToArray(), "Changed offset");

                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    _createdObjects[i].transform.position = GetDefaultPositionAtIndex(i);
                }
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            GameObject proxy = GetProxy();

            if (_createdObjects.Count > 0 && proxy != null)
            {
                Vector3 offset = (Vector3)_offset * index;
                return proxy.transform.position + offset;
            }

            Debug.LogError($"Proxy not found for Array Creator. Positions may not appear correctly");
            return default;
        }

        private void OnOffsetChange()
        {
            UpdatePositions();
        }

        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                GameObject clone = GameObject.Instantiate(_target, _target.transform.position, _target.transform.rotation, _target.transform.parent);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

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
        }

        protected override ArrayData GetContainerData()
        {
            LinearArrayData data = new LinearArrayData(_target, _targetRotation);
            data.Count = TargetCount;
            data.Offset = _offset;
            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is LinearArrayData lineData)
            {
                SetTargetCount(lineData.Count);
                _offset.Set(lineData.Offset);
                _targetRotation = lineData.TargetRotation;
            }
        }

        protected override void OnTargetCountChanged()
        {
            if (TargetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > TargetCount)
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
                while (TargetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }
        }

        protected override string[] GetAllowedModifiers()
        {
            string[] mods =
            {
                ModifierType.RotationRandom,
                ModifierType.ScaleRandom,
                ModifierType.ScaleUniform,
                ModifierType.RotationRandom,
                ModifierType.RotationUniform,
                ModifierType.IncrementalRotation,
                ModifierType.IncrementalScale,
                ModifierType.PositionNoise,
            };

            return mods;
        }

    }
}
