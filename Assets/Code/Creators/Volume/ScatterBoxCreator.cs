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

        private Shared<Vector3> _center = new Shared<Vector3>();
        private Vector3Property _centerProperty = null;
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
                GameObject clone = GameObject.Instantiate(_target, position, _target.transform.rotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

                _positions.Add(position);
                _createdObjects.Add(clone);
            }
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

            _center.Set(_centerProperty.Update());
            _size.Set(_sizeProperty.Update());

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
