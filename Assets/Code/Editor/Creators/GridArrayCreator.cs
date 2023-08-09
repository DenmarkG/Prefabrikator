using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class GridArrayCreator : ArrayCreator
    {
        public override ShapeType Shape => ShapeType.Grid;
        public enum Dimension
        {
            XY,
            XZ,
            YZ,
            XYZ
        }

        public override int MinCount => 2;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Grid";

        private Shared<Vector3> _center = new Shared<Vector3>();
        private Vector3Property _centerProperty = null;

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

        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

        public GridArrayCreator(GameObject target)
            : base(target, DefaultCount * DefaultCount)
        {
            _center.Set(target.transform.position);
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

                GUILayout.Space(Extensions.IndentSize);
                EditorGUILayout.LabelField("Offsets", EditorStyles.boldLabel);
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

                GUILayout.Space(Extensions.IndentSize / 2);
                _center.Set(_centerProperty.Update());
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
            if (_createdObjects.Count > 0)
            {
                int index = 0;
                GameObject currentObj = null;
                if (_dimension != Dimension.XYZ)
                {
                    Vector3 rowDirection = _dimension == Dimension.YZ ? Vector3.forward : Vector3.right;
                    Vector3 colDirection = _dimension == Dimension.XZ ? Vector3.forward : Vector3.up;

                    int rowCount = _dimension == Dimension.YZ ? _countZ : _countX;
                    int colCount = _dimension == Dimension.XZ ? _countZ : _countY;

                    float rowOffsetDistance = _dimension == Dimension.YZ ? _offsetZ : _offsetX;
                    float colOffsetDistance = _dimension == Dimension.XZ ? _offsetZ: _offsetY;

                    float length = (rowCount - 1) * rowOffsetDistance;
                    float height = (colCount - 1) * colOffsetDistance;

                    Vector3 rowStart = _center.Get() - ((length / 2f) * rowDirection);
                    Vector3 colStart = _center.Get() - ((height / 2f) * colDirection);

                    for (int x = 0; x < rowCount; ++x)
                    {
                        Vector3 posX = rowStart + (rowDirection * rowOffsetDistance * x);
                        
                        for (int y = 0; y < colCount; ++y)
                        {
                            Vector3 posY = colStart + (colDirection * colOffsetDistance * y);
                            Vector3 position = posX + posY;
                            currentObj = _createdObjects[index];
                            currentObj.transform.position = position;
                            ++index;
                        }
                    }
                }
                else
                {
                    float length = (_countX - 1) * _offsetX;
                    float height = (_countY - 1) * _offsetY;
                    float depth = (_countZ - 1) * _offsetZ;

                    Vector3 rowStart = _center.Get() - ((length / 2f) * Vector3.right);
                    Vector3 colStart = _center.Get() - ((height / 2f) * Vector3.up);
                    Vector3 depthStart = _center.Get() - ((depth / 2f) * Vector3.forward);
                    for (int z = 0; z < _countZ; ++z)
                    {
                        Vector3 posZ = depthStart + (Vector3.forward * _offsetZ * z);

                        for (int x = 0; x < _countX; ++x)
                        {
                            Vector3 posX = rowStart + (Vector3.right * _offsetX * x);

                            for (int y = 0; y < _countY; ++y)
                            {
                                Vector3 posY = colStart + (Vector3.up * _offsetY * y);

                                currentObj = _createdObjects[index];
                                currentObj.transform.position = posX + posY + posZ;
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
            void OnCenterChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_center, previous, current));
            }
            _centerProperty = new Vector3Property("Center", _center, OnCenterChanged);
            _centerProperty.OnEditModeEnter += () => { _editMode |= EditMode.Center; };
            _centerProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Center; };

            void OnXChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_offsetX, previous, current));
            }
            _xOffsetProperty = new FloatProperty("X", _offsetX, OnXChanged);
            _xOffsetProperty.OnEditModeEnter += () => { _editMode |= EditMode.OffsetX; };
            _xOffsetProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.OffsetX; };

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
            _yOffsetProperty.OnEditModeEnter += () => { _editMode |= EditMode.OffsetY; };
            _yOffsetProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.OffsetY; };

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
            _zOffsetProperty.OnEditModeEnter += () => { _editMode |= EditMode.OffsetZ; };
            _zOffsetProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.OffsetZ; };

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
            if (_editMode != EditMode.None && proxy != null)
            {
                Vector3 size = new Vector3();
                size.x = _offsetX * (ShouldShowX() ? (_countX - 1) : 0);
                size.y = _offsetY * (ShouldShowY() ? (_countY - 1) : 0);
                size.z = _offsetZ * (ShouldShowZ() ? (_countZ - 1) : 0);
                _boundsHandle.size = size;

                _boundsHandle.center = _center;
                _boundsHandle.SetColor(Color.cyan);

                Vector3 center = _center;

                EditorGUI.BeginChangeCheck();
                {
                    EditMode offsetFlag = EditMode.OffsetX | EditMode.OffsetY | EditMode.OffsetZ;

                    if ((_editMode & offsetFlag) != 0)
                    {
                        _boundsHandle.DrawHandle();
                    }

                    if (_editMode.HasFlag(EditMode.Center))
                    {
                        center = Handles.PositionHandle(_center, Quaternion.identity);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (_editMode.HasFlag(EditMode.OffsetX))
                    {
                        _offsetX.Set(_boundsHandle.size.x / (_countX - 1));
                    }
                    
                    if (_editMode.HasFlag(EditMode.OffsetY))
                    {
                        _offsetY.Set(_boundsHandle.size.y / (_countY - 1));
                    }
                    
                    if (_editMode.HasFlag(EditMode.OffsetZ))
                    {
                        _offsetZ.Set(_boundsHandle.size.z / (_countZ - 1));
                    }

                    if (_editMode.HasFlag(EditMode.Center))
                    {
                        _center.Set(center);
                    }
                }
            }
        }

        protected override string[] GetAllowedModifiers()
        {
            if (_dimension == Dimension.XYZ)
            {
                return base.GetAllowedModifiers();
            }

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
