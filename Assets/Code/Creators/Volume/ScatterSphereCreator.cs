using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class ScatterSphereCreator : ScatterVolumeCreator
    {
        public override ShapeType Shape => ShapeType.ScatterSphere;
        private static readonly float DefaultRadius = 5f;
        
        private Shared<float> _radius = new Shared<float>(DefaultRadius);
        private FloatProperty _radiusProperty = null;

        private SphereBoundsHandle _sphereHandle = new SphereBoundsHandle();

        public ScatterSphereCreator(GameObject target)
            : base(target)
        {
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
                GameObject clone = GameObject.Instantiate(_target, (position.Value * _radius) + _center, _target.transform.rotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

                _positions.Add(position.Value);
                _createdObjects.Add(clone);
            }
        }

        protected override void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions.Clear();

            int count = _createdObjects.Count;

            for (int i = 0; i < count; ++i)
            {
                Vector3? position = GetRandomPointInBounds();
                _positions.Add(position.Value);
            }

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                int count = positions.Length;
                ApplyToAll((go, index) => { go.transform.position = (_positions[index] * _radius) + _center; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
        }

        protected override void DrawVolumeEditor()
        {
            Vector3 center = _centerProperty.Update();
            if (center != _center)
            {
                MarkDirty();
                _center.Set(center);
            }

            float radius = _radiusProperty.Update();
            if (radius != _radius)
            {
                MarkDirty();
                _radius.Set(radius);
            }

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        protected override ArrayState GetContainerData()
        {
            // #DG: TODO
            return null;
        }

        protected override void PopulateFromExistingData(ArrayState data)
        {
            // #DG: TODO
        }

        protected override Vector3 GetRandomPointInBounds()
        {
            return Random.insideUnitSphere;
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

            void OnRadiusChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_radius, previous, current));
            }
            _radiusProperty = new FloatProperty("Radius", _radius, OnRadiusChanged);
            _radiusProperty.OnEditModeEnter += () => { _editMode |= EditMode.Size; };
            _radiusProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Size; };
        }

        protected override void UpdatePositions()
        {
            int count = _createdObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                _createdObjects[i].transform.position = (_positions[i] * _radius) + _center;
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

                _sphereHandle.center = center;
                _sphereHandle.radius = _radius;

                EditorGUI.BeginChangeCheck();
                {
                    if (_editMode.HasFlag(EditMode.Size))
                    {
                        _sphereHandle.DrawHandle();
                    }

                    if (_editMode.HasFlag(EditMode.Center))
                    {
                        center = Handles.PositionHandle(center, Quaternion.identity);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    MarkDirty();

                    if (_sphereHandle.radius != _radius)
                    {
                        _radius.Set(_sphereHandle.radius);
                    }

                    if (center != _center)
                    {
                        _center.Set(center);
                    }
                }
            }
            else
            {
                Handles.DrawWireDisc(_center, Vector3.up, _radius);
                //Handles.DrawWireDisc(_center, Vector3.right, _radius);
                Handles.DrawWireDisc(_center, Vector3.forward, _radius);
            }
        }

        public override void OnStateSet(ArrayState stateData)
        {
            throw new System.NotImplementedException();
        }
    }
}
