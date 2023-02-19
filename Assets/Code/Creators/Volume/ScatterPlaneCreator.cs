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
        public class ScatterPlaneData : ArrayState
        {
            public ScatterPlaneData()
                : base(ShapeType.ScatterPlane)
            {
                //
            }
        }

        public override ShapeType Shape => ShapeType.ScatterPlane;

        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

        private Shared<Vector3> _size = new Shared<Vector3>(new Vector3(10f, 0f, 10f));
        private Vector3Property _sizeProperty = null;

        public ScatterPlaneCreator(GameObject target) 
            : base(target)
        {
            _size.Set(new Vector3(10f, 0f, 10f));
            _center.Set(target.transform.position);
            SetupProperties();
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

        protected override ArrayState GetContainerData()
        {
            return default;
        }


        protected override Vector3 GetRandomPointInBounds()
        {
            return GetRandomPoisson() ?? Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }

        protected override void PopulateFromExistingData(ArrayState data)
        {
            throw new NotImplementedException();
        }

        protected override void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions = ScatterPoisson();

            while (_positions.Count < _createdObjects.Count)
            {
                _positions.Add(GetRandomPointInBounds());
            }

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                ApplyToAll((go, index) => { go.transform.position = _positions[index]; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
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

        protected override void UpdatePositions()
        {
            int count = _createdObjects.Count;
            
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.position = _positions[i];
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

        public override ArrayState GetState()
        {
            var stateData = new ScatterPlaneData() 
            { 
                Count = TargetCount,
            };

            stateData.Positions = new Vector3[stateData.Count];
            for (int i = 0; i < stateData.Count; ++i)
            {
                stateData.Positions[i] = _createdObjects[i].transform.position;
            }


            return stateData;
        }

        public override void OnStateSet(ArrayState stateData)
        {
            if (stateData is ScatterPlaneData data)
            {
                SetTargetCount(data.Count, shouldTriggerCallback: false);

                Debug.Assert(data.Count == _createdObjects.Count, "Counts are not equal, cannot apply state change");

                _positions = new List<Vector3>(data.Count);
                for (int i = 0; i < data.Count; ++i)
                {
                    _positions.Add(data.Positions[i]);
                    _createdObjects[i].transform.position = data.Positions[i];
                }
            }
        }

        protected override void SetupProperties()
        {
            base.SetupProperties();
            void OnCenterChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_center, previous, current));
            }
            _centerProperty = new Vector3Property("Center", _center, OnCenterChanged);
            _centerProperty.OnEditModeEnter += () => { _editMode |= EditMode.Center; };
            _centerProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Center; };

            void OnSizeChanged(Vector3 current, Vector3 previous)
            {
                if (current.y != 0f)
                {
                    current.y = 0f;
                    _size.Set(current);
                }

                CommandQueue.Enqueue(new GenericCommand<Vector3>(_size, previous, current));
            }
            _sizeProperty = new Vector3Property("Size", _size, OnSizeChanged);
            _sizeProperty.OnEditModeEnter += () => { _editMode |= EditMode.Size; };
            _sizeProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Size; };
        }

        protected override Vector3 GetInitialPosition()
        {
            return Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }

        protected override Dimension GetDimension()
        {
            return Dimension.Two;
        }
    }
}

