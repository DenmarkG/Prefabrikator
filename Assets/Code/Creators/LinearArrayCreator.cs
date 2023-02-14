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

        public LinearArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
            void OnValueSet(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_offset, previous, current));
            };
            _offsetProperty = new Vector3Property("Offset", _offset, OnValueSet);
            _offsetProperty.OnEditModeEnter += () => { _editMode = EditMode.Position; };
            _offsetProperty.OnEditModeExit += () => { _editMode = EditMode.None; };

            Refresh();
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        _offset.Set(_offsetProperty.Update());
                    }
                    EditorGUILayout.EndHorizontal();
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

        protected override bool CreateClone(int index = 0)
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
                return true;
            }

            return false;
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

            
            if (_editMode.HasFlag(EditMode.Position))
            {
                GameObject proxy = GetProxy();
                Handles.color = Color.green;
                const float offsetHeight = 2f;
                Vector3 verticalOffset = offsetHeight * Vector3.up;
                Vector3 start = proxy.transform.position;
                Vector3 end = start + (_offset.Get() * (_createdObjects.Count - 1));

                Handles.DrawLine(start, end);
                Handles.DrawLine(start, start + verticalOffset);
                Handles.DrawLine(end, end + verticalOffset);

                Handles.CapFunction cap = Handles.SphereHandleCap;
                int startID = GUIUtility.GetControlID(FocusType.Passive);
                
                Vector3 startHndPos = start + verticalOffset;
                const float handleSize = .75f;
                Vector3 start2 = Handles.FreeMoveHandle(startHndPos, handleSize, Vector3.zero, cap) - verticalOffset;

                Vector3 endHndPos = end + verticalOffset;
                Vector3 end2 = Handles.FreeMoveHandle(endHndPos, handleSize, Vector3.zero, cap) - verticalOffset;

                if (start2 != start || end2 != end)
                {
                    proxy.transform.position = start2;
                    _offset.Set((end2 - start2) / (_createdObjects.Count - 1));
                }
            }
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
            };

            return mods;
        }
    }
}
