using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

using Random = UnityEngine.Random;

namespace Prefabrikator
{
    public class ScatterPlaneCreator : ScatterVolumeCreator
    {
        public override ShapeType Shape => ShapeType.ScatterPlane;

        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

        private Shared<Vector3> _size = new Shared<Vector3>(new Vector3(10f, 0f, 10f));
        protected Vector3Property _sizeProperty = null;
        protected virtual Shared<Vector3> DefaultSize => new Shared<Vector3>(new Vector3(10f, 0f, 10f));
        private Vector3 _centerDefault = Vector3.zero;

        public ScatterPlaneCreator(GameObject target) 
            : base(target)
        {
            //_size.Set(new Vector3(10f, 0f, 10f));
            _size.Set(DefaultSize);
            _center.Set(target.transform.position);
            SetupProperties();
        }

        public override Bounds CalculateBounds()
        {
            return new Bounds(_center, _size);
        }

        protected override void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

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
                        for (int i = 0; i < _positions.Count; ++i)
                        {
                            _positions[i] += center - _center;
                        }

                        ApplyToAll((obj, index) => obj.transform.position = _positions[index]);

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

            SetSceneViewDirty();
        }


        protected override Vector3 GetRandomPointInBounds()
        {
            return GetRandomPoisson() ?? Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            base.OnRefreshStart(hardRefresh, useDefaultData);

            if (_positions.Count != _createdObjects.Count)
            {
                int countDiff = _createdObjects.Count - _positions.Count;
                if (countDiff > 0)
                {
                    for (int i = _positions.Count - 1; i < _createdObjects.Count; ++i)
                    {
                        _positions.Add(_createdObjects[i].transform.position);
                    }
                }
                else
                {
                    while (_positions.Count != _createdObjects.Count)
                    {
                        _positions.RemoveAt(_positions.Count - 1);
                    }
                }
            }
        }

        protected override bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint)
        {
            Bounds testBounds = new Bounds(_center, _size);

            if (scatteredPoints.Count > 0)
            {
                foreach (Vector3 point in scatteredPoints)
                {
                    if (IsValidPoint(testBounds, point, testPoint) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return testBounds.Contains(testPoint);
            }
        }

        private bool IsValidPoint(Bounds testBounds, Vector3 activePoint, Vector3 testPoint)
        {
            if (!testBounds.Contains(testPoint))
            {
                return false;
            }

            float distance = Vector3.Distance(activePoint, testPoint);

            if (distance < _scatterRadius)
            {
                return false;
            }

            return true;
        }

        protected override void SetupProperties()
        {
            base.SetupProperties();
            void OnCenterChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_center, previous, current));
            }
            _centerProperty = new Vector3Property("Center", _center, OnCenterChanged);
            _centerProperty.OnEditModeEnter += OnEnterCenterEdit;
            _centerProperty.OnEditModeExit += OnExitCenterEdit;

            void OnSizeChanged(Vector3 current, Vector3 previous)
            {
                _size.Set(EnforceSizeConstraints(current));

                CommandQueue.Enqueue(new GenericCommand<Vector3>(_size, previous, current));
            }
            _sizeProperty = new Vector3Property("Size", _size, OnSizeChanged);
            _sizeProperty.OnEditModeEnter += () => { _editMode |= EditMode.Size; };
            _sizeProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Size; };
        }

        private void OnEnterCenterEdit()
        {
            _editMode |= EditMode.Center;
            _centerDefault = _center;
        }

        private void OnExitCenterEdit(ExitMode exitMode)
        {
            _editMode &= ~EditMode.Center;

            if (exitMode == ExitMode.Cancel)
            {
                Vector3 moveDelta = _center - _centerDefault;
                OnPositionsChanged(moveDelta);
            }
        }

        private void OnPositionsChanged(Vector3 moveDelta)
        {
            for (int i = 0; i < _positions.Count; ++i)
            {
                _positions[i] -= moveDelta;
            }

            ApplyToAll((obj, index) => obj.transform.position = _positions[index]);
        }

        protected override Vector3 GetInitialPosition()
        {
            return Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }

        protected override Dimension GetDimension()
        {
            return Dimension.Two;
        }

        protected virtual Vector3 EnforceSizeConstraints(Vector3 newSize)
        {
            if (newSize.y != 0f)
            {
                newSize.y = 0f;
            }

            return newSize;
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
                ModifierType.PositionNoise,
                ModifierType.DropToFloor,
            };

            return mods;
        }
    }
}

