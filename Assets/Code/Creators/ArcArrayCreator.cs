﻿using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class ArcArrayData : ArrayData
    {
        public float FillPercent = ArcArrayCreator.DefaultFillPercent;
        public bool CapEnd = false;
        public float Radius = CircularArrayCreator.DefaultRadius;

        public ArcArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Arc, prefab, targetRotation)
        {
            Count = CircularArrayCreator.MinCount;
        }
    }

    public class ArcArrayCreator : CircularArrayCreator
    {
        public override float MaxWindowHeight => 400f;
        public override string Name => "Arc";

        // how much of circle to fill; makes arcs possible
        public static readonly float DefaultFillPercent = .375f;
        private float _fillPercent = DefaultFillPercent;

        private ArcHandle _arcHandle = new ArcHandle();

        public ArcArrayCreator(GameObject target)
            : base(target)
        {
            _arcHandle.SetColorWithRadiusHandle(Color.gray, .25f);
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
            {
                EditorGUILayout.LabelField("Fill", GUILayout.Width(Extensions.LabelWidth));
                float fillPercent = EditorGUILayout.Slider(_fillPercent, 0f, .9999f, null);
                if (fillPercent != _fillPercent)
                {
                    _fillPercent = fillPercent;
                    //_needsRefresh = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            base.DrawEditor();
        }

        protected override void UpdatePositions()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                Vector3 position = GetDefaultPositionAtIndex(i);
                _createdObjects[i].transform.localPosition = position + _center;
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            float degrees = (360f * _fillPercent) * Mathf.Deg2Rad; // #DG: TODO multiply this by fill percent
            int n = _createdObjects.Count - 1;
            float angle = (n != 0f) ? (degrees / n) : 0f;

            float t = angle * index;
            float x = Mathf.Cos(t) * _radius;
            float z = Mathf.Sin(t) * _radius;

            return new Vector3(x, _target.transform.position.y, z);
        }

        protected override ArrayData GetContainerData()
        {
            ArcArrayData data = new ArcArrayData(_target, Quaternion.identity);
            data.Count = TargetCount;
            data.Radius = _radius;
            data.FillPercent = _fillPercent;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is ArcArrayData arcData)
            {
                SetTargetCount(arcData.Count);
                _radius.Set(arcData.Radius);
                _fillPercent = arcData.FillPercent;
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

                _arcHandle.angle = 360f * _fillPercent;
                _arcHandle.radius = _radius;
                _arcHandle.radiusHandleDrawFunction = Handles.CubeHandleCap;

                Vector3 handleDirection = Vector3.right;
                Vector3 handleNormal = Vector3.Cross(handleDirection, Vector3.forward);
                Matrix4x4 handleMatrix = Matrix4x4.TRS(
                    _center,
                    Quaternion.LookRotation(handleDirection, handleNormal),
                    Vector3.one
                );

                using (new Handles.DrawingScope(handleMatrix))
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        center = Handles.PositionHandle(_center, Quaternion.identity);
                        _arcHandle.DrawHandle();
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (center != _center)
                    {
                        _center.Set(center);
                    }

                    float fillPercent = _arcHandle.angle / 360f;
                    if (!Mathf.Approximately(_fillPercent, fillPercent))
                    {
                        _fillPercent = fillPercent;
                    }

                    if (_arcHandle.radius != _radius)
                    {
                        _radius.Set(_arcHandle.radius);
                    }
                }
            }
        }

        protected override string[] GetAllowedModifiers()
        {
            string[] mods =
            {
                ModifierType.RotationRandom,
                ModifierType.ScaleRandom,
                ModifierType.ScaleUniform,
                ModifierType.RotationRandom,
                ModifierType.RotationUniform,
                ModifierType.IncrementalRotation,
                ModifierType.IncrementalScale,
                ModifierType.PositionNoise,
                // #DG: add circle specic mods here
                ModifierType.FollowCurve,
            };

            return mods;
        }
    }
}
