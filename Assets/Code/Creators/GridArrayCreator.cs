using UnityEngine;
using UnityEditor;
using System;

namespace Prefabrikator
{
    public class GridArrayData : ArrayData
    {
        public GridArrayCreator.Dimension Dimension = GridArrayCreator.Dimension.XY;
        public Vector3 OffsetVector = GridArrayCreator.DefaultOffset;
        public Vector3Int CountVector = GridArrayCreator.DefaultCount;

        public GridArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ArrayType.Grid, prefab, targetRotation)
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

        public static readonly Vector3 DefaultOffset = new Vector3(2, 2, 2);
        private Shared<Vector3> _offsetVector = new Shared<Vector3>(DefaultOffset);

        public static readonly Vector3Int DefaultCount = new Vector3Int(3, 3, 3);
        private Vector3Int _countVector = DefaultCount;

        private Dimension _dimension = Dimension.XY;

        private bool _needsPositionRefresh = false;

        private CountProperty _xCountProperty = null;
        private CountProperty _yCountProperty = null;
        private CountProperty _zCountProperty = null;

        private FloatProperty _xOffset = null;
        private FloatProperty _yOffset = null;
        private FloatProperty _zOffset = null;

        public GridArrayCreator(GameObject target)
            : base(target)
        {
            SetupCountProperties();
            SetupOffsetProperties();
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical(_boxedHeaderStyle);
            {
                Dimension dimension = (Dimension)EditorGUILayout.EnumPopup(_dimension);
                if (dimension != _dimension)
                {
                    _dimension = dimension;
                    // #DG: Fix this 
                    //_needsRefresh = true;
                    _needsPositionRefresh = true;
                }

                int targetCount = 1;

                if (ShouldShowX())
                {
                    _countVector.x = _xCountProperty.Update();
                    targetCount *= (_countVector.x > 0) ? _countVector.x : 1;
                }

                if (ShouldShowY())
                {
                    _countVector.y = _yCountProperty.Update();
                    targetCount *= (_countVector.y > 0) ? _countVector.y : 1;
                }

                if (ShouldShowZ())
                {
                    _countVector.z = _zCountProperty.Update();
                    targetCount *= (_countVector.z > 0) ? _countVector.z : 1;
                }

                if (targetCount != _targetCount)
                {
                    _targetCount = targetCount;
                }


                EditorGUILayout.LabelField("Offsets");
                if (ShouldShowX())
                {
                    using (new EditorGUI.DisabledGroupScope(_countVector.x == 0))
                    {
                        Vector3 offset = _offsetVector.Get();
                        offset.x = _xOffset.Update();
                        _offsetVector.Set(offset);
                    }
                }

                if (ShouldShowY())
                {
                    using (new EditorGUI.DisabledGroupScope(_countVector.y == 0))
                    {
                        Vector3 offset = _offsetVector.Get();
                        offset.y = _yOffset.Update();
                        _offsetVector.Set(offset);
                    }
                }

                if (ShouldShowZ())
                {
                    using (new EditorGUI.DisabledScope(_countVector.z == 0))
                    {
                        Vector3 offset = _offsetVector.Get();
                        offset.z = _zOffset.Update();
                        _offsetVector.Set(offset);
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

            if (_needsPositionRefresh)
            {
                ResetAllPositions();
            }

            EstablishHelper();

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
            if (_createdObjects.Count > 0)
            {
                int index = 0;
                GameObject currentObj = null;
                if (_dimension != Dimension.XYZ)
                {
                    Vector3 rowVector = _dimension == Dimension.YZ ? Vector3.forward : Vector3.right;
                    Vector3 colVector = _dimension == Dimension.XZ ? Vector3.forward : Vector3.up;

                    int rowCount = _dimension == Dimension.YZ ? _countVector.z : _countVector.x;
                    int colCount = _dimension == Dimension.XZ ? _countVector.z : _countVector.y;

                    for (int x = 0; x < rowCount; ++x)
                    {
                        Vector3 offsetX = rowVector * (((Vector3)_offsetVector).x * x);
                        currentObj = _createdObjects[index];
                        currentObj.transform.position = _targetProxy.transform.position + offsetX;
                        ++index;

                        for (int y = 1; y < colCount; ++y)
                        {
                            Vector3 offsetY = (colVector * (((Vector3)_offsetVector).y * y)) + offsetX;
                            currentObj = _createdObjects[index];
                            currentObj.transform.position = _targetProxy.transform.position + offsetY;
                            ++index;
                        }
                    }
                }
                else
                {
                    for (int z = 0; z < _countVector.z; ++z)
                    {
                        Vector3 offsetZ = Vector3.forward * (((Vector3)_offsetVector).z * z);

                        for (int x = 0; x < _countVector.x; ++x)
                        {
                            Vector3 offsetX = (Vector3.right * (((Vector3)_offsetVector).x * x)) + offsetZ;
                            currentObj = _createdObjects[index];
                            currentObj.transform.position = _targetProxy.transform.position + offsetX;
                            ++index;

                            for (int y = 1; y < _countVector.y; ++y)
                            {
                                Vector3 offsetY = (Vector3.up * (((Vector3)_offsetVector).y * y)) + offsetX;
                                currentObj = _createdObjects[index];
                                currentObj.transform.position = _targetProxy.transform.position + offsetY;
                                ++index;
                            }
                        }
                    }
                }
            }
        }

        private void ResetAllPositions()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.position = _targetProxy.transform.position;
            }

            _needsPositionRefresh = false;
        }

        private int GetCount()
        {
            int targetCount = 1;
            targetCount *= (_countVector.x > 0) ? _countVector.x : 1;
            targetCount *= (_countVector.y > 0) ? _countVector.y : 1;
            targetCount *= (_countVector.z > 0) ? _countVector.z : 1;

            return targetCount;
        }

        private void SetupCountProperties()
        {
            void OnXChanged(int current, int previous)
            {
                var valueCommand = new ValueChangedCommand<int>(previous, current, (x) =>
                {
                    _countVector.x = x;
                });

                CommandQueue.Enqueue(valueCommand);
            }
            _xCountProperty = new CountProperty("Rows", _countVector.x, OnXChanged);

            void OnYChanged(int current, int previous)
            {
                var valueCommand = new ValueChangedCommand<int>(previous, current, (y) =>
                {
                    _countVector.y = y;
                });

                CommandQueue.Enqueue(valueCommand);
            }
            _yCountProperty = new CountProperty("Columns", _countVector.y, OnYChanged);

            void OnZChanged(int current, int previous)
            {
                var valueCommand = new ValueChangedCommand<int>(previous, current, (z) =>
                {
                    _countVector.z = z;
                });

                CommandQueue.Enqueue(valueCommand);
            }
            _zCountProperty = new CountProperty("Depth", _countVector.z, OnZChanged);
        }

        private void SetupOffsetProperties()
        {
            void OnXChanged(float current, float previous)
            {
                var valueCommand = new ValueChangedCommand<float>(previous, current, (x) =>
                {
                    Vector3 offset = _offsetVector.Get();
                    offset.x = x;
                    _offsetVector.Set(offset);
                });

                CommandQueue.Enqueue(valueCommand);
            }
            _xOffset = new FloatProperty("X", ((Vector3)_offsetVector).x, OnXChanged);

            void OnYChanged(float current, float previous)
            {
                var valueCommand = new ValueChangedCommand<float>(previous, current, (y) =>
                {
                    Vector3 offset = _offsetVector.Get();
                    offset.y = y;
                    _offsetVector.Set(offset);
                });

                CommandQueue.Enqueue(valueCommand);
            }
            _yOffset = new FloatProperty("Y", _offsetVector.Get().z, OnYChanged);

            void OnZChanged(float current, float previous)
            {
                var valueCommand = new ValueChangedCommand<float>(previous, current, (z) =>
                {
                    Vector3 offset = _offsetVector.Get();
                    offset.z = z;
                    _offsetVector.Set(offset);
                });

                CommandQueue.Enqueue(valueCommand);
            }
            _zOffset = new FloatProperty("Z", _offsetVector.Get().z, OnZChanged);
        }

        private void CreateClone()
        {
            GameObject clone = GameObject.Instantiate(_target, _target.transform.position, _target.transform.rotation, _target.transform.parent);
            clone.transform.SetParent(_targetProxy.transform);
            _createdObjects.Add(clone);
        }

        protected override ArrayData GetContainerData()
        {
            GridArrayData data = new GridArrayData(_target, _targetRotation);
            data.Count = GetCount();
            data.CountVector = _countVector;
            data.Dimension = _dimension;
            data.OffsetVector = _offsetVector;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is GridArrayData gridData)
            {
                _countVector = gridData.CountVector;
                _dimension = gridData.Dimension;
                _offsetVector.Set(gridData.OffsetVector);
                _targetRotation = gridData.TargetRotation;
                _targetCount = gridData.Count;
            }
        }
    }
}
