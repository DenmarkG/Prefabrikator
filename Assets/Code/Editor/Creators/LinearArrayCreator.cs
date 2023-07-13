using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class LinearArrayData : ArrayState
    {
        public Vector3 Offset;

        public LinearArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Line)
        {
            //
        }
    }

    public class LinearArrayCreator : ArrayCreator
    {
        public override ShapeType Shape => ShapeType.Line;
        public override int MinCount => 2;
        public static readonly int DefaultCount = 5;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Line";

        private Shared<Vector3> _offset = new Shared<Vector3>(new Vector3(2f, 0f, 0f));
        private Vector3Property _offsetProperty = null;

        private Shared<Vector3> _start = new Shared<Vector3>();
        private Vector3Property _startProperty = null;

        public LinearArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
            _start.Set(target.transform.position);
            SetupProperties();

            Refresh();
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _start.Set(_startProperty.Update());
                    GameObject proxy = GetProxy();
                    proxy.transform.position = _start;

                    _offset.Set(_offsetProperty.Update());
                }

                ShowCountField();
            }
            EditorGUILayout.EndVertical();

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
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
                return _start + offset;
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

        protected override ArrayState GetContainerData()
        {
            LinearArrayData data = new LinearArrayData(_target, _targetRotation);
            data.Count = TargetCount;
            data.Offset = _offset;
            return data;
        }

        protected override void PopulateFromExistingData(ArrayState data)
        {
            if (data is LinearArrayData lineData)
            {
                SetTargetCount(lineData.Count);
                _offset.Set(lineData.Offset);
            }
        }

        protected override void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            GameObject proxy = GetProxy();

            if (IsEditMode)
            {
                if (_editMode.HasFlag(EditMode.Position))
                {
                    Handles.color = Color.green;
                    Vector3 start = proxy.transform.position;
                    Vector3 end = start + (_offset.Get() * (_createdObjects.Count - 1));

                    Handles.DrawLine(start, end);

                    Vector3 endHndPos = end;
                    Vector3 end2 = Handles.PositionHandle(endHndPos, Quaternion.identity);

                    if (end2 != end)
                    {
                        _offset.Set((end2 - start) / (_createdObjects.Count - 1));
                    }
                }

                if (_editMode.HasFlag(EditMode.Center))
                {
                    Handles.color = Color.cyan;
                    Vector3 start = Handles.PositionHandle(proxy.transform.position, Quaternion.identity);

                    if (start != _start)
                    {
                        _start.Set(start);
                        proxy.transform.position = start;
                    }
                }
            }
        }

        public void SetupProperties()
        {
            _startProperty = Vector3Property.Create("Start", _start, CommandQueue);
            _startProperty.OnEditModeEnter += () => { _editMode |= EditMode.Center; };
            _startProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Center; };

            _offsetProperty = Vector3Property.Create("Offset", _offset, CommandQueue);
            _offsetProperty.OnEditModeEnter += () => { _editMode |= EditMode.Position; };
            _offsetProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Position; };
        }

        public override void OnStateSet(ArrayState stateData)
        {
            if (stateData is LinearArrayData data)
            {
                _offset.Set(data.Offset);
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
                ModifierType.DropToFloor,
            };

            return mods;
        }
    }
}
