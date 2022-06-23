﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Prefabrikator
{
    public class GridArrayData : ArrayData
    {
        public GridArrayCreator.Dimension Dimension = GridArrayCreator.Dimension.XY;
        //public Vector3 OffsetVector = GridArrayCreator.DefaultOffset;

        public GridArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Grid, prefab, targetRotation)
        {
            //
        }
    }

    public class GridArrayCreator : ArrayCreator
    {
        public enum Dimension
        {
            XY,
            XZ,
            YZ,
            XYZ
        }

        public override float MaxWindowHeight => 300f;
        public override string Name => "Grid";

        public static readonly float DefaultOffset = 2f;
        private Shared<float> _offsetX = new Shared<float>(DefaultOffset);
        private Shared<float> _offsetY = new Shared<float>(DefaultOffset);
        private Shared<float> _offsetZ = new Shared<float>(DefaultOffset);

        private const int DefaultCount = 3;
        private Shared<int> _countX = new Shared<int>(DefaultCount);
        private Shared<int> _countY = new Shared<int>(DefaultCount);
        private Shared<int> _countZ = new Shared<int>(DefaultCount);

        private Shared<Dimension> _dimension = new Shared<Dimension>(Dimension.XY);

        private bool _needsPositionRefresh = false;

        private FloatProperty _xOffsetProperty = null;
        private FloatProperty _yOffsetProperty = null;
        private FloatProperty _zOffsetProperty = null;

        private List<Vector3> _defaultPositions = new List<Vector3>();

        private Shared<bool> _useCheckerboard = new Shared<bool>(false);
        private BoolProperty _checkerboardProperty = null;
        private bool _showCheckerboardOptions = false;

        private enum OffsetDirection
        {
            Row,
            Column,
        }

        private Shared<OffsetDirection> _offsetDirection = new Shared<OffsetDirection>(OffsetDirection.Row);
        private Shared<Vector3> _checkerboardOffset = new Shared<Vector3>(new Vector3(1f, 0f, 0f));
        private Vector3Property _checkerboardOffsetProperty = null;

        public GridArrayCreator(GameObject target)
            : base(target, DefaultCount * DefaultCount)
        {
            SetupProperties();
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical(Extensions.BoxedHeaderStyle);
            {
                Dimension dimension = (Dimension)EditorGUILayout.EnumPopup(_dimension);
                if (dimension != _dimension)
                {
                    CommandQueue.Enqueue(new GenericCommand<Dimension>(_dimension, _dimension.Get(), dimension));
                    // #DG: Fix this 
                    //_needsRefresh = true;
                    _needsPositionRefresh = true;
                }

                int targetCount = 1;

                _useCheckerboard.Set(_checkerboardProperty.Update());
                if (_useCheckerboard)
                {
                    _showCheckerboardOptions = EditorGUILayout.Foldout(_showCheckerboardOptions, "Checkerboard Options");
                    if (_showCheckerboardOptions)
                    {
                        OffsetDirection direction = (OffsetDirection)EditorGUILayout.EnumPopup("Offset Type", _offsetDirection);
                        if (direction != _offsetDirection)
                        {
                            CommandQueue.Enqueue(new GenericCommand<OffsetDirection>(_offsetDirection, _offsetDirection.Get(), direction));
                            _needsPositionRefresh = true;
                        }

                        _checkerboardOffset.Set(_checkerboardOffsetProperty.Update());
                    }
                }

                GUILayout.Space(5);

                if (ShouldShowX())
                {
                    int countX = _countX;
                    if (Extensions.DisplayCountField(ref countX))
                    {
                        CommandQueue.Enqueue(new GenericCommand<int>(_countX, _countX, countX));
                    }
                    targetCount *= (_countX > 0) ? _countX : 1;
                }

                if (ShouldShowY())
                {
                    int countY = _countY;
                    if (Extensions.DisplayCountField(ref countY))
                    {
                        CommandQueue.Enqueue(new GenericCommand<int>(_countY, _countY, countY));
                    }
                    targetCount *= (_countY > 0) ? _countY : 1;
                }

                if (ShouldShowZ())
                {
                    int countZ = _countZ;
                    if (Extensions.DisplayCountField(ref countZ))
                    {
                        CommandQueue.Enqueue(new GenericCommand<int>(_countZ, _countZ, countZ));
                    }
                    targetCount *= (_countZ > 0) ? _countZ : 1;
                }

                if (targetCount != _targetCount)
                {
                    _targetCount = targetCount;
                }


                EditorGUILayout.LabelField("Offsets");
                if (ShouldShowX())
                {
                    using (new EditorGUI.DisabledGroupScope(_countX == 0))
                    {
                        _offsetX.Set(_xOffsetProperty.Update());
                    }
                }

                if (ShouldShowY())
                {
                    using (new EditorGUI.DisabledGroupScope(_countY == 0))
                    {
                        _offsetY.Set(_yOffsetProperty.Update());
                    }
                }

                if (ShouldShowZ())
                {
                    using (new EditorGUI.DisabledScope(_countZ == 0))
                    {
                        _offsetZ.Set(_zOffsetProperty.Update());
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private bool ShouldShowX() => _dimension != Dimension.YZ;
        private bool ShouldShowY() => _dimension != Dimension.XZ;
        private bool ShouldShowZ() => _dimension != Dimension.XY;

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
            }

            // Update the target count
            {
                int targetCount = GetCount();

                if (targetCount != _targetCount)
                {
                    _targetCount = targetCount;
                }
            }

            if (_targetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }

            if (_needsPositionRefresh)
            {
                ResetAllPositions();
            }
            UpdatePositions();

            EstablishHelper();

            UpdateLocalRotations();
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (NeedsRefresh)
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

        private void OnOffsetChange()
        {
            UpdatePositions();
        }

        private void OnCountChange()
        {
            ResetAllPositions();
            Refresh();
        }

        private void UpdatePositions()
        {
            GameObject proxy = GetProxy();
            if (_createdObjects.Count > 0 && proxy != null)
            {
                float direction = 1f;

                int index = 0;
                GameObject currentObj = null;
                if (_dimension != Dimension.XYZ)
                {
                    Vector3 rowVector = _dimension == Dimension.YZ ? Vector3.forward : Vector3.right;
                    Vector3 colVector = _dimension == Dimension.XZ ? Vector3.forward : Vector3.up;

                    int rowCount = _dimension == Dimension.YZ ? _countZ : _countX;
                    int colCount = _dimension == Dimension.XZ ? _countZ : _countY;

                    float rowOffset = _dimension == Dimension.YZ ? _offsetZ : _offsetX;
                    float colOffset = _dimension == Dimension.XZ ? _offsetZ: _offsetY;

                    for (int x = 0; x < rowCount; ++x)
                    {
                        Vector3 offsetX = rowVector * (rowOffset * x);
                        currentObj = _createdObjects[index];
                        currentObj.transform.position = proxy.transform.position + offsetX;
                        ++index;

                        for (int y = 1; y < colCount; ++y)
                        {
                            Vector3 offsetY = (colVector * (colOffset * y)) + offsetX;
                            currentObj = _createdObjects[index];
                            currentObj.transform.position = proxy.transform.position + offsetY;
                            ++index;
                        }
                    }
                }
                else
                {
                    for (int z = 0; z < _countZ; ++z)
                    {
                        Vector3 offsetZ = Vector3.forward * (_offsetZ * z);

                        for (int x = 0; x < _countX; ++x)
                        {
                            Vector3 offsetX = (Vector3.right * (_offsetX * x)) + offsetZ;
                            currentObj = _createdObjects[index];
                            currentObj.transform.position = proxy.transform.position + offsetX;
                            ++index;

                            for (int y = 1; y < _countY; ++y)
                            {
                                Vector3 offsetY = (Vector3.up * (_offsetY * y)) + offsetX;
                                currentObj = _createdObjects[index];
                                currentObj.transform.position = proxy.transform.position + offsetY;
                                ++index;
                            }
                        }
                    }
                }

                int count = _createdObjects.Count;
                if (_defaultPositions.Count != count)
                {
                    _defaultPositions = new List<Vector3>();
                    for (int i = 0; i < count; ++i)
                    {
                        _defaultPositions.Add(_createdObjects[i].transform.position);
                    }
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        _defaultPositions[i] = (_createdObjects[i].transform.position);
                    }
                }
            }
        }

        private void ResetAllPositions()
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    _createdObjects[i].transform.position = proxy.transform.position;
                }

                _needsPositionRefresh = false;
            }
        }

        private int GetCount()
        {
            int targetCount = 1;
            targetCount *= (_countX > 0 && ShouldShowX()) ? _countX : 1;
            targetCount *= (_countY > 0 && ShouldShowY()) ? _countY : 1;
            targetCount *= (_countZ > 0 && ShouldShowZ()) ? _countZ : 1;

            return targetCount;
        }

        private void SetupProperties()
        {
            void OnXChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_offsetX, previous, current));
            }
            _xOffsetProperty = new FloatProperty("X", _offsetX, OnXChanged);

            void OnYChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_offsetY, previous, current));
            }
            _yOffsetProperty = new FloatProperty("Y", _offsetY, OnYChanged);

            void OnZChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_offsetZ, previous, current));
            }
            _zOffsetProperty = new FloatProperty("Z", _offsetZ, OnZChanged);

            void OnCheckerboardChange(bool current, bool previous)
            {
                CommandQueue.Enqueue(new GenericCommand<bool>(_useCheckerboard, previous, current));
            }
            _checkerboardProperty = new BoolProperty("Checkerboard", _useCheckerboard, OnCheckerboardChange);

            void OnOffsetChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_checkerboardOffset, previous, current));
            }
            _checkerboardOffsetProperty = new Vector3Property("Offset", _checkerboardOffset, OnOffsetChanged);
        }

        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                GameObject clone = GameObject.Instantiate(_target, _target.transform.position, _target.transform.rotation, _target.transform.parent);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);
                _createdObjects.Add(clone);
            }
        }

        protected override ArrayData GetContainerData()
        {
            GridArrayData data = new GridArrayData(_target, _targetRotation);
            data.Count = GetCount();
            //data.CountVector = _countVector;
            data.Dimension = _dimension;
            //data.OffsetVector = _offsetVector;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is GridArrayData gridData)
            {
                //_countVector = gridData.CountVector;
                //_dimension = gridData.Dimension;
                //_offsetVector.Set(gridData.OffsetVector);
                _targetRotation = gridData.TargetRotation;
                _targetCount = gridData.Count;
            }
        }

        protected override void OnTargetCountChanged()
        {
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
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return _defaultPositions[index];
        }
    }
}
