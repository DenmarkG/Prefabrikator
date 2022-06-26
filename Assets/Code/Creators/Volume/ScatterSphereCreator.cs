using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class ScatterSphereCreator : ScatterVolumeCreator
    {
        private static readonly float DefaultRadius = 5f;
        
        private Shared<float> _radius = new Shared<float>(DefaultRadius);
        private FloatProperty _radiusProperty = null;
        private Shared<Vector3> _center = new Shared<Vector3>();
        private Vector3Property _centerProperty = null;

        private SphereBoundsHandle _sphereHandle = new SphereBoundsHandle();

        public ScatterSphereCreator(GameObject target)
            : base(target)
        {
            _center.Set(target.transform.position);

            SetupProperties();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        ~ScatterSphereCreator()
        {
            Teardown();
        }

        protected override void OnSave()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public override void Teardown()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
            base.Teardown();
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

        protected override void DrawVolumeEditor()
        {
            int currentCount = _targetCount;
            if (Extensions.DisplayCountField(ref currentCount))
            {
                CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, Mathf.Max(currentCount, MinCount)));
            }

            _center.Set(_centerProperty.Update());
            _radius.Set(_radiusProperty.Update());

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        protected override ArrayData GetContainerData()
        {
            // #DG: TODO
            return null;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            // #DG: TODO
        }

        protected override Vector3 GetRandomPointInBounds()
        {
            return Random.insideUnitSphere * _radius;
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

        private void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            Debug.Log($"Edit mode = {_editMode}");
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
    }
}
