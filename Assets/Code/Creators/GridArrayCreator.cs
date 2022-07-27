using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

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

        public override int MinCount => DefaultCount;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Grid";

        public static readonly float DefaultOffset = 2f;
        private Shared<float> _offsetX = new Shared<float>(DefaultOffset);
        private Shared<float> _offsetY = new Shared<float>(DefaultOffset);
        private Shared<float> _offsetZ = new Shared<float>(DefaultOffset);

        private static readonly int DefaultCount = 3;
        private Shared<int> _countX = new Shared<int>(DefaultCount);
        private IntProperty _xCountProperty = null;

        private Shared<int> _countY = new Shared<int>(DefaultCount);
        private IntProperty _yCountProperty = null;

        private Shared<int> _countZ = new Shared<int>(DefaultCount);
        private IntProperty _zCountProperty = null;

        private Shared<Dimension> _dimension = new Shared<Dimension>(Dimension.XY);

        private bool _needsPositionRefresh = false;

        private FloatProperty _xOffsetProperty = null;
        private FloatProperty _yOffsetProperty = null;
        private FloatProperty _zOffsetProperty = null;

        private List<Vector3> _defaultPositions = new List<Vector3>();

        private int _editsEnabled = 0;
        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

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

                GUILayout.Space(5);

                // #DG: These need labels
                if (ShouldShowX())
                {
                    _countX.Set(_xCountProperty.Update());
                    targetCount *= (_countX > 0) ? _countX : 1;
                }

                if (ShouldShowY())
                {
                    _countY.Set(_yCountProperty.Update());
                    targetCount *= (_countY > 0) ? _countY : 1;
                }

                if (ShouldShowZ())
                {
                    _countZ.Set(_zCountProperty.Update());
                    targetCount *= (_countZ > 0) ? _countZ : 1;
                }

                if (targetCount != TargetCount)
                {
                    SetTargetCount(targetCount);
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

            SetSceneViewDirty();
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

                if (targetCount != TargetCount)
                {
                    SetTargetCount(targetCount);
                }
            }

            if (TargetCount != _createdObjects.Count)
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
                if (_createdObjects.Count != TargetCount)
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
            _xOffsetProperty.OnEditModeEnter += () => { ++_editsEnabled; };
            _xOffsetProperty.OnEditModeExit += () => { --_editsEnabled; };

            void OnXCountChange(int current, int previous)
            {
                current = EnforceValidCount(current);
                CommandQueue.Enqueue(new GenericCommand<int>(_countX, previous, current));
            }
            _xCountProperty = new IntProperty("Count X", _countX, OnXCountChange, EnforceValidCount);
            _xCountProperty.AddCustomButton(Constants.PlusButton, (value) => { _countX.Set(++value); });
            _xCountProperty.AddCustomButton(Constants.MinusButton, (value) => { _countX.Set(--value); });

            void OnYChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_offsetY, previous, current));
            }
            _yOffsetProperty = new FloatProperty("Y", _offsetY, OnYChanged);
            _yOffsetProperty.OnEditModeEnter += () => { ++_editsEnabled; };
            _yOffsetProperty.OnEditModeExit += () => { --_editsEnabled; };

            void OnYCountChange(int current, int previous)
            {
                current = EnforceValidCount(current);
                CommandQueue.Enqueue(new GenericCommand<int>(_countY, previous, current));
            }
            _yCountProperty = new IntProperty("Count Y", _countY, OnYCountChange, EnforceValidCount);
            _yCountProperty.AddCustomButton(Constants.PlusButton, (value) => { _countY.Set(++value); });
            _yCountProperty.AddCustomButton(Constants.MinusButton, (value) => { _countY.Set(--value); });

            void OnZChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_offsetZ, previous, current));
            }
            _zOffsetProperty = new FloatProperty("Z", _offsetZ, OnZChanged);
            _zOffsetProperty.OnEditModeEnter += () => { ++_editsEnabled; };
            _zOffsetProperty.OnEditModeExit += () => { --_editsEnabled; };

            void OnZCountChange(int current, int previous)
            {
                current = EnforceValidCount(current);
                CommandQueue.Enqueue(new GenericCommand<int>(_countZ, previous, current));
            }
            _zCountProperty = new IntProperty("Count Z", _countZ, OnZCountChange, EnforceValidCount);
            _zCountProperty.AddCustomButton(Constants.PlusButton, (value) => { _countZ.Set(++value); });
            _zCountProperty.AddCustomButton(Constants.MinusButton, (value) => { _countZ.Set(--value); });
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
                SetTargetCount(gridData.Count);
            }
        }

        protected override void OnTargetCountChanged()
        {
            if (TargetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > TargetCount)
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
                while (TargetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return _defaultPositions[index];
        }

        protected override void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            GameObject proxy = GetProxy();
            if (_editsEnabled > 0 && proxy != null)
            {
                Vector3 size = new Vector3();
                size.x = _offsetX * _countX;
                size.y = _offsetY * _countY;
                size.z = _offsetZ * _countZ;
                _boundsHandle.size = size;

                Vector3 center = (size / 2f) + proxy.transform.position;
                _boundsHandle.center = center;                

                EditorGUI.BeginChangeCheck();
                {
                    _boundsHandle.DrawHandle();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    //
                }
            }
        }
    }
}
