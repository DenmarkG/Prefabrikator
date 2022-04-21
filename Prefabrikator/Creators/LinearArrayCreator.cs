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
        public static readonly int MinCount = 2;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Line";
        private Vector3 _offset = new Vector3(2, 0, 0);

        private PropertyExtensions.Vector3Property _offsetProperty = null;

        public LinearArrayCreator(GameObject target)
            : base(target)
        {
            _targetCount = MinCount;
            _needsRefresh = true;

            _offsetProperty = new PropertyExtensions.Vector3Property("Offset", _offset);
        }

        private bool _changeStarted = false;

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                // #DG: add a popup that can update this value
                // only update when the popup apply button is pressed
                // if cancel is pressed, don't apply changes
                EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
                {
                    Vector3 offset = _offsetProperty.Update();
                    if (offset != _offset)
                    {
                        CommandQueue.Enqueue(new OnOffsetChangeCommand(this, _offset, offset));
                    }


                    //EditorGUI.BeginChangeCheck();
                    //EditorGUILayout.LabelField("Offset", GUILayout.Width(ArrayToolExtensions.LabelWidth));
                    //Vector3 offset = EditorGUILayout.Vector3Field(string.Empty, _offset, null);
                    //if (EditorGUI.EndChangeCheck())
                    //{
                    //    CommandQueue.Enqueue(new OnOffsetChangeCommand(this, _offset, offset));

                    //    //OnOffsetChangeCommand offsetChange = null;
                    //    //if (!_changeStarted)
                    //    //{
                    //    //    _changeStarted = true;
                    //    //    offsetChange = new OnOffsetChangeCommand(this, _offset, offset);
                    //    //}
                    //    //else
                    //    //{
                    //    //    if (CommandQueue != null && CommandQueue.Count > 0)
                    //    //    {
                    //    //        if (CommandQueue.Peek() is OnOffsetChangeCommand command)
                    //    //        {
                    //    //            offsetChange = command;
                    //    //            offsetChange.NextOffset = offset;
                    //    //        }
                    //    //    }
                    //    //}

                    //    //CommandQueue.Enqueue(offsetChange);
                    //}
                    //else
                    //{
                    //    if (!_changeStarted)
                    //    {
                    //        _changeStarted = false;
                    //    }
                    //}
                }
                EditorGUILayout.EndHorizontal();

                int currentTargetCount = _targetCount;
                if (ArrayToolExtensions.DisplayCountField(ref currentTargetCount))
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
                if (_needsRefresh || CommandQueue.Count > 0)
                {
                    Refresh();
                }

                // Update positions
                OnOffsetChange();
            }
        }

        public override void Refresh(bool hardRefresh = false, bool useDefaultData = false)
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
                UpdateLocalScales();
                UpdateLocalRotations();

                _needsRefresh = false;
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
                }
            }

            public override void Revert()
            {
                if (Creator is LinearArrayCreator linearCreator)
                {
                    linearCreator._offset = PreviousOffset;
                }
            }
        }
    }
    #endregion // Commands
}
