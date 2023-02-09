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
        private const int MaxSamples = 30;

        public override ShapeType Shape => ShapeType.ScatterPlane;
        private static readonly Vector3 DefaultSize = new Vector3(10f, 0f, 10f);

        private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();

        private Shared<Vector3> _size = new Shared<Vector3>(DefaultSize);
        private Vector3Property _sizeProperty = null;

        private Shared<float> _radius = new Shared<float>(2f);
        private Vector3Property _initalPositionProperty = null;
        private Shared<Vector3> _initialPosition = new Shared<Vector3>();

        public ScatterPlaneCreator(GameObject target) 
            : base(target)
        {
            _center.Set(target.transform.position);
            SetupProperties();
        }

        protected override bool CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Vector3? position = GetRandomPointInBounds();
                if (position != null)
                {
                    //Vector3 relativePos = ConvertPointToShapeRelative(position);

                    GameObject clone = GameObject.Instantiate(_target, position.Value, _target.transform.rotation);
                    clone.SetActive(true);
                    clone.transform.SetParent(proxy.transform);

                    _positions.Add(position.Value);
                    _createdObjects.Add(clone);
                    return true;
                }
            }

            return false;
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

        protected override ArrayData GetContainerData()
        {
            return default;
        }

        protected override Vector3? GetRandomPointInBounds()
        {
            Bounds bounds = new Bounds(_center, _size);
            if (_createdObjects.Count == 0)
            {
                return Extensions.GetRandomPointInBounds(bounds);
            }

            foreach (GameObject activeObject in _createdObjects)
            {
                Vector3 initialSample = activeObject.transform.position;
                Vector3[] samplePoints = GenerateSampleSet(initialSample, _radius, 2f * _radius);
                foreach (Vector3 sample in samplePoints)
                {
                    Vector3 testPosition = sample + initialSample;
                    if (IsValidPoint(_positions, testPosition))
                    {
                        return testPosition;
                    }
                }
            }

            return null;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            throw new NotImplementedException();
        }

        protected override void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions = ScatterPoisson();

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                ApplyToAll((go, index) => { go.transform.position = _positions[index]; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
        }

        protected override void UpdatePositions()
        {
            int count = _createdObjects.Count;
            
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.position = _positions[i];
            }
        }

        private List<Vector3> ScatterPoisson(Vector3? initialPosition = null)
        {
            List<Vector3> scatteredPoints = new();

            List<Vector3> activePoints = new(TargetCount);

            // #DG: Make this a user controlled variable
            Vector3 initialSample = initialPosition ?? Extensions.GetRandomPointInBounds(new Bounds(_center, _size));

            activePoints.Add(initialSample);

            while (activePoints.Count > 0 && scatteredPoints.Count < TargetCount)
            {
                bool sampleFound = false;
                Vector3[] samplePoints = GenerateSampleSet(initialSample, _radius, 2f * _radius);
                foreach (Vector3 sample in samplePoints)
                {
                    Vector3 testPosition = sample + initialSample;

                    if (IsValidPoint(scatteredPoints, testPosition))
                    {
                        activePoints.Add(testPosition);
                        scatteredPoints.Add(testPosition);

                        sampleFound = true;
                        break;
                    }
                }

                if (!sampleFound)
                {
                    activePoints.Remove(initialSample);
                }

                if (activePoints.Count > 0)
                {
                    initialSample = activePoints[Random.Range(0, activePoints.Count)];
                }
            }

            Debug.Log($"Created {scatteredPoints.Count} / {TargetCount} points");
            return scatteredPoints;
        }

        private bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint)
        {
            Bounds testBounds = new Bounds(_center, _size);
            foreach (Vector3 point in scatteredPoints)
            {
                if (IsValidPoint(testBounds, point, testPoint) ==  false)
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
            
            if (distance < _radius)
            {
                Debug.Log($"Distance = {distance}");
                return false;
            }

            return true;
        }

        private Vector3[] GenerateSampleSet(Vector3 center, float minRadius, float maxRadius)
        {
            Vector3[] samples = new Vector3[MaxSamples];
            for (int i = 0; i < MaxSamples; ++i)
            {
                Vector2 random = Random.insideUnitCircle;
                Vector3 direction = new Vector3(random.x, 0f, random.y);
                direction *= Random.Range(minRadius, maxRadius);
                samples[i] = direction;
            }

            return samples;
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
                    if (CreateClone() == false)
                    {
                        DecrementTargetCount();
                    }
                }
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

