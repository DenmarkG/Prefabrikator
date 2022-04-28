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

    // #DG: TODO - Add Bidirectional option
    public class LinearArrayCreator : ArrayCreator
    {
        public static readonly int MinCount = 2;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Line";
        private Vector3 _offset = new Vector3(2, 0, 0);

        private Vector3Property _offsetProperty = null;

        public LinearArrayCreator(GameObject target)
            : base(target)
        {
            _targetCount = MinCount;

            void OnValueSet(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new OnOffsetChangeCommand(this, previous, current));
            };

            _offsetProperty = new Vector3Property("Offset", _offset, OnValueSet);

            Refresh();
        }

        private bool _changeStarted = false;

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
                {
                    _offset = _offsetProperty.Update();
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
            if (!_changeStarted)
            {
                ICommand nextCommand = null;
                while (CommandQueue.Count > 0)
                {
                    nextCommand = CommandQueue.Dequeue();
                    ExecuteCommand(nextCommand);
                }

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

                ProcessModifiers();
            }
        }

        private void UpdatePositions()
        {
            
            if (_createdObjects.Count > 0)
            {
                Undo.RecordObjects(_createdObjects.ToArray(), "Changed offset");
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
                _offset = lineData.Offset;
                _targetRotation = lineData.TargetRotation;
            }
        }

        #region Commands
        internal class OnOffsetChangeCommand : CreatorCommand
        {
            public Vector3 PreviousOffset { get; set;  }
            public Vector3 NextOffset { get; set; }

            public OnOffsetChangeCommand(ArrayCreator creator, Vector3 previousOffset, Vector3 nextOffset)
                : base(creator)
            {
                PreviousOffset = previousOffset;
                NextOffset = nextOffset;
            }

            public override void Execute()
            {
                if (Creator is LinearArrayCreator linearCreator)
                {
                    linearCreator._offset = NextOffset;
                    linearCreator._offsetProperty.SetDefaultValue(linearCreator._offset);
                }
            }

            public override void Revert()
            {
                if (Creator is LinearArrayCreator linearCreator)
                {
                    linearCreator._offset = PreviousOffset;
                    linearCreator._offsetProperty.SetDefaultValue(linearCreator._offset);
                }
            }
        }
    }
    #endregion // Commands
}
