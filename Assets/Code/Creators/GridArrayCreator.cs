using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class GridArrayData : ArrayData
    {
        public GridArrayCreator.Dimension Dimension = GridArrayCreator.Dimension.XY;
        public Vector3 OffsetVector = GridArrayCreator.DefaultOffset;
        public Vector3Int CountVector = GridArrayCreator.DefaultCount;

        public GridArrayData(GameObject prefab, Vector3 targetScale, Quaternion targetRotation)
            : base(ArrayType.Grid, prefab, targetScale, targetRotation)
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
        private Vector3 _offsetVector = DefaultOffset;

        public static readonly Vector3Int DefaultCount = new Vector3Int(3, 3, 3);
        private Vector3Int _countVector = DefaultCount;

        private Dimension _dimension = Dimension.XY;

        private bool _needsPositionRefresh = false;

        public GridArrayCreator(GameObject target)
            : base(target)
        {
            //
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical(_boxedHeaderStyle);
            {
                Dimension dimension = (Dimension)EditorGUILayout.EnumPopup(_dimension);
                if (dimension != _dimension)
                {
                    _dimension = dimension;
                    _needsRefresh = true;
                    _needsPositionRefresh = true;
                }

                int targetCount = 1;

                if (ShouldShowX())
                {
                    int countX = _countVector.x;
                    if (ArrayToolExtensions.DisplayCountField(ref countX, "Rows"))
                    {
                        _countVector.x = Mathf.Max(countX, 0);
                        _needsRefresh = true;
                        _needsPositionRefresh = true;
                    }
                    targetCount *= (_countVector.x > 0) ? _countVector.x : 1;
                }

                if (ShouldShowY())
                {
                    int countY = _countVector.y;
                    if (ArrayToolExtensions.DisplayCountField(ref countY, "Columns"))
                    {
                        _countVector.y = Mathf.Max(countY, 0);
                        _needsRefresh = true;
                        _needsPositionRefresh = true;
                    }
                    targetCount *= (_countVector.y > 0) ? _countVector.y : 1;
                }

                if (ShouldShowZ())
                {
                    int countZ = _countVector.z;
                    if (ArrayToolExtensions.DisplayCountField(ref countZ, "Depth"))
                    {
                        _countVector.z = Mathf.Max(countZ, 0);
                        _needsRefresh = true;
                        _needsPositionRefresh = true;
                    }
                    targetCount *= (_countVector.z > 0) ? _countVector.z : 1;
                }

                if (targetCount != _targetCount)
                {
                    _targetCount = targetCount;
                    _needsRefresh = true;
                }

                if (ShouldShowX())
                {
                    using (new EditorGUI.DisabledGroupScope(_countVector.x == 0))
                    {
                        float offsetX = EditorGUILayout.FloatField("Offset X", _offsetVector.x);
                        if (offsetX != _offsetVector.x)
                        {
                            _offsetVector.x = offsetX;
                            _needsRefresh = true;
                        }
                    }
                }

                if (ShouldShowY())
                {
                    using (new EditorGUI.DisabledGroupScope(_countVector.y == 0))
                    {
                        float offsetY = EditorGUILayout.FloatField("Offset Y", _offsetVector.y);
                        if (offsetY != _offsetVector.y)
                        {
                            _offsetVector.y = offsetY;
                            _needsRefresh = true;
                        }
                    }
                }

                if (ShouldShowZ())
                {
                    using (new EditorGUI.DisabledScope(_countVector.z == 0))
                    {
                        float offsetZ = EditorGUILayout.FloatField("Offset Z", _offsetVector.z);
                        if (offsetZ != _offsetVector.z)
                        {
                            _offsetVector.z = offsetZ;
                            _needsRefresh = true;
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private bool ShouldShowX() => _dimension != Dimension.YZ;
        private bool ShouldShowY() => _dimension != Dimension.XZ;
        private bool ShouldShowZ() => _dimension != Dimension.XY;

        public override void Refresh(bool hardRefresh = false, bool useDefaultData = false)
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
            UpdateLocalScales();
            UpdateLocalRotations();
            _needsRefresh = false;
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (_needsRefresh)
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
                        Vector3 offsetX = rowVector * (_offsetVector.x * x);
                        currentObj = _createdObjects[index];
                        currentObj.transform.position = _targetProxy.transform.position + offsetX;
                        ++index;

                        for (int y = 1; y < colCount; ++y)
                        {
                            Vector3 offsetY = (colVector * (_offsetVector.y * y)) + offsetX;
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
                        Vector3 offsetZ = Vector3.forward * (_offsetVector.z * z);

                        for (int x = 0; x < _countVector.x; ++x)
                        {
                            Vector3 offsetX = (Vector3.right * (_offsetVector.x * x)) + offsetZ;
                            currentObj = _createdObjects[index];
                            currentObj.transform.position = _targetProxy.transform.position + offsetX;
                            ++index;

                            for (int y = 1; y < _countVector.y; ++y)
                            {
                                Vector3 offsetY = (Vector3.up * (_offsetVector.y * y)) + offsetX;
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

        private void CreateClone()
        {
            GameObject clone = GameObject.Instantiate(_target, _target.transform.position, _target.transform.rotation, _target.transform.parent);
            clone.transform.SetParent(_targetProxy.transform);
            _createdObjects.Add(clone);
        }

        protected override ArrayData GetContainerData()
        {
            GridArrayData data = new GridArrayData(_target, _targetScale, _targetRotation);
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
                _offsetVector = gridData.OffsetVector;
                _targetScale = gridData.TargetScale;
                _targetRotation = gridData.TargetRotation;
                _targetCount = gridData.Count;
            }
        }
    }
}
