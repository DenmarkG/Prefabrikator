using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class ScatterBoxCreator : ScatterVolumeCreator
    {
        private static readonly Vector3 DefaultSize = new Vector3(5f, 2f, 5f);

        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

        private Shared<Vector3> _size = new Shared<Vector3>(DefaultSize);
        private Vector3Property _sizeProperty = null;

        public ScatterBoxCreator(GameObject target)
            : base(target)
        {
            _center.Set(target.transform.position);

            SetupProperties();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        ~ScatterBoxCreator()
        {
            Teardown();
        }

        protected override void OnSave()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public override void Teardown()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
            base.Teardown();
        }

        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Vector3 position = GetRandomPointInBounds();

                Vector3 extents = (_size.Get() / 2f);
                Vector3 min = _center - extents;
                Vector3 max = _center + extents;

                Vector3 relativePos = new Vector3();
                relativePos.x = position.x.Normalize(min.x, max.x);
                relativePos.y = position.y.Normalize(min.y, max.y);
                relativePos.z = position.z.Normalize(min.z, max.z);

                GameObject clone = GameObject.Instantiate(_target, position, _target.transform.rotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

                _positions.Add(relativePos);
                _createdObjects.Add(clone);
            }
        }

        protected override void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions.Clear();

            int count = _createdObjects.Count;

            for (int i = 0; i < count; ++i)
            {
                Vector3 position = GetRandomPointInBounds();
                _createdObjects[i].transform.position = position;
                _positions.Add(position);
            }

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                int count = positions.Length;
                ApplyToAll((go, index) => { go.transform.position = _positions[index]; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
        }

        protected override ArrayData GetContainerData()
        {
            // #DG: TODO
            return null;
        }

        protected override Vector3 GetRandomPointInBounds()
        {
            return Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            // #DG: TODO
        }

        protected override void UpdatePositions()
        {
            int count = _createdObjects.Count;
            Vector3 extents = (_size.Get() / 2f);
            Vector3 min = _center - extents;
            Vector3 max = _center + extents;

            for (int i = 0; i < count; ++i)
            {
                Vector3 relPos = _positions[i];
                float x = Mathf.Lerp(min.x, max.x, relPos.x);
                float y = Mathf.Lerp(min.y, max.y, relPos.y);
                float z = Mathf.Lerp(min.z, max.z, relPos.z);

                _createdObjects[i].transform.position = new Vector3(x, y, z);
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            // #DG: wrap this in an edit mode boolean
            if (IsEditMode)
            {
                Vector3 center = _center;

                _boundsHandle.center = center;
                _boundsHandle.size = _size;

                EditorGUI.BeginChangeCheck();
                {
                    if (_editMode.HasFlag(EditMode.Size))
                    {
                        _boundsHandle.DrawHandle();
                    }

                    if (_editMode.HasFlag(EditMode.Center))
                    {
                        center = Handles.PositionHandle(center, Quaternion.identity);
                    }
                    
                }
                if (EditorGUI.EndChangeCheck())
                {
                    MarkDirty();
                    if (_boundsHandle.size != _size)
                    {
                        _size.Set(_boundsHandle.size);
                    }

                    if (center != _center)
                    {
                        _center.Set(center);
                    }
                }
            }
            else
            {
                Handles.DrawWireCube(_center, _size);
            }
        }

        protected override void DrawVolumeEditor()
        {
            int currentCount = _targetCount;
            if (Extensions.DisplayCountField(ref currentCount))
            {
                CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, Mathf.Max(currentCount, MinCount)));
            }

            Vector3 center = _centerProperty.Update();
            if (center != _center)
            {
                MarkDirty();
                _center.Set(center);
            }

            Vector3 size = _sizeProperty.Update();
            if (size != _size)
            {
                MarkDirty();
                _size.Set(size);
            }

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        private void SetupProperties()
        {
            void OnCenterChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_center, previous, current));
            }
            _centerProperty = new Vector3Property("Center", _center, OnCenterChanged);
            _centerProperty.OnEditModeEnter += () => { _editMode |= EditMode.Center; };
            _centerProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Center; };

            void OnSizeChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_size, previous, current));
            }
            _sizeProperty = new Vector3Property("Size", _size, OnSizeChanged);
            _sizeProperty.OnEditModeEnter += () => { _editMode |= EditMode.Size; };
            _sizeProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Size; };
        }
    }
}
