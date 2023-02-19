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

        private float SqRadius => _radius * _radius;

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
            return GetRandomPoisson() ?? Extensions.RandomInsideSphere(_radius);
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

            void OnRadiusChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_radius, previous, current));
            }
            _radiusProperty = new FloatProperty("Radius", _radius, OnRadiusChanged);
            _radiusProperty.OnEditModeEnter += () => { _editMode |= EditMode.Size; };
            _radiusProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Size; };
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

        protected override bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint)
        {
            if (scatteredPoints.Count > 0)
            {
                foreach (Vector3 point in scatteredPoints)
                {
                    if (IsValidPoint(point, testPoint) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return (testPoint - _center).sqrMagnitude > SqRadius;
            }
        }

        private bool IsValidPoint(Vector3 activePoint, Vector3 testPoint)
        {
            if ((testPoint - _center).sqrMagnitude > SqRadius)
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
            return Extensions.RandomInsideSphere(_radius);
        }

        protected override Dimension GetDimension()
        {
            return Dimension.Three;
        }
    }
}
