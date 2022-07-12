using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class EllipseArrayCreator : CircularArrayCreator
    {
        public float ZRadius => _zRadius;
        private Shared<float> _zRadius = new Shared<float>(7.5f);
        private FloatProperty _zRadiusProperty = null;

        public float XRadius => _radius;
        private SphereBoundsHandle _xRadiusHandle = new SphereBoundsHandle();
        private SphereBoundsHandle _zRadiusHandle = new SphereBoundsHandle();

        public EllipseArrayCreator(GameObject target) 
            : base(target)
        {
            void OnZRadiusSet(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_zRadius, previous, current));
            }
            _zRadiusProperty = new FloatProperty("Z Radius", _zRadius, OnZRadiusSet);

            _xRadiusHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Z;
            _zRadiusHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Z;
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                _center.Set(_centerProperty.Update());
                _radius.Set(Mathf.Abs(_radiusProperty.Update()));
                _zRadius.Set(Mathf.Abs(_zRadiusProperty.Update()));

                int currentCount = _targetCount;
                if (Extensions.DisplayCountField(ref currentCount))
                {
                    CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, Mathf.Max(currentCount, MinCount)));
                }
            }
            EditorGUILayout.EndVertical();

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            GameObject proxy = GetProxy();

            const float degrees = Mathf.PI * 2;
            float angle = (degrees / _createdObjects.Count);

            float t = angle * index;
            float x = Mathf.Cos(t) * _radius;
            float z = Mathf.Sin(t) * _zRadius;

            return new Vector3(x, proxy.transform.position.y, z) + _center;
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

                _xRadiusHandle.center = center;
                _xRadiusHandle.radius = _radius;

                _zRadiusHandle.center = center;
                _zRadiusHandle.radius = _zRadius;
                

                EditorGUI.BeginChangeCheck();
                {
                    center = Handles.PositionHandle(_center, Quaternion.identity);
                    _xRadiusHandle.DrawHandle();
                    _zRadiusHandle.DrawHandle();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (center != _center)
                    {
                        _center.Set(center);
                    }

                    if (_xRadiusHandle.radius != _radius)
                    {
                        _radius.Set(_xRadiusHandle.radius);
                    }

                    if (_zRadiusHandle.radius != _zRadius)
                    {
                        _zRadius.Set(_zRadiusHandle.radius);
                    }
                }
            }
        }
    }
}

