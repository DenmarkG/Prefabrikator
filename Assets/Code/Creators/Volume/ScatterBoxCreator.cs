using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class ScatterBoxCreator : ScatterVolumeCreator
    {
        public override ShapeType Shape => ShapeType.ScatterBox;

        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();
        private Shared<Vector3> _size = new Shared<Vector3>(new Vector3(5f, 5f, 5f));
        private Vector3Property _sizeProperty = null;

        public ScatterBoxCreator(GameObject target)
            : base(target)
        {
            _size.Set(new Vector3(5f, 2f, 5f));
            _center.Set(target.transform.position);

            SetupProperties();
        }

        protected override void OnSave()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Vector3? position = GetRandomPointInBounds();

                if (position != null)
                {
                    Vector3 relativePos = ConvertPointToShapeRelative(position.Value);

                    GameObject clone = GameObject.Instantiate(_target, position.Value, _target.transform.rotation);
                    clone.SetActive(true);
                    clone.transform.SetParent(proxy.transform);

                    _positions.Add(relativePos);
                    _createdObjects.Add(clone);
                }
            }
        }

        private Vector3 ConvertPointToShapeRelative(Vector3 point)
        {
            Vector3 extents = (_size.Get() / 2f);
            Vector3 min = _center - extents;
            Vector3 max = _center + extents;

            Vector3 relativePos = new Vector3();
            relativePos.x = point.x.Normalize(min.x, max.x);
            relativePos.y = point.y.Normalize(min.y, max.y);
            relativePos.z = point.z.Normalize(min.z, max.z);

            return relativePos;
        }

        private Vector3 ConvertPointToWorldRelative(Vector3 point)
        {
            Vector3 extents = (_size.Get() / 2f);
            Vector3 min = _center - extents;
            Vector3 max = _center + extents;
            
            float x = Mathf.Lerp(min.x, max.x, point.x);
            float y = Mathf.Lerp(min.y, max.y, point.y);
            float z = Mathf.Lerp(min.z, max.z, point.z);

            return new Vector3(x, y, z);
        }

        protected override void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions = ScatterPoisson();

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                ApplyToAll((go, index) => { go.transform.position = ConvertPointToWorldRelative(_positions[index]); });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
        }

        protected override ArrayState GetContainerData()
        {
            // #DG: TODO
            return null;
        }

        protected override Vector3 GetRandomPointInBounds()
        {
            return Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }


        protected override void PopulateFromExistingData(ArrayState data)
        {
            // #DG: TODO
        }

        protected override void UpdatePositions()
        {
            int count = _createdObjects.Count;

            for (int i = 0; i < count; ++i)
            {
                _createdObjects[i].transform.position = ConvertPointToWorldRelative(_positions[i]);
            }
        }

        protected override void OnSceneGUI(SceneView view)
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

        public override void OnStateSet(ArrayState stateData)
        {
            //
        }

        protected override bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint)
        {
            Bounds testBounds = new Bounds(_center, _size);
            foreach (Vector3 point in scatteredPoints)
            {
                if (IsValidPoint(testBounds, point, testPoint) == false)
                {
                    return false;
                }
            }

            return true;
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

        protected override Vector3 GetInitialPosition()
        {
            return Extensions.GetRandomPointInBounds(new Bounds(_center, _size));
        }

        protected override Dimension GetDimension()
        {
            return Dimension.Three;
        }
    }
}
