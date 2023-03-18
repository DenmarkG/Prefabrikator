﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
    using SharedPoint = Shared<ControlPoint>;

    public class BezierArrayCreator : ArrayCreator
    {
        private static readonly int DefaultCount = 3;
        public override int MinCount => DefaultCount;

        public CubicBezierCurve Curve => _curve;
        private CubicBezierCurve _curve = new CubicBezierCurve();

        public override float MaxWindowHeight => 300f;

        public override string Name => "Path";

        public override ShapeType Shape => ShapeType.Path;

        private List<SharedPoint> _controlPoints = new List<SharedPoint>();
        private List<BezierProperty> _properties = new List<BezierProperty>();

        public BezierArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
            SceneView.duringSceneGui += OnSceneGUI;

            _controlPoints.Add(new SharedPoint(ControlPoint.Default));
            _controlPoints.Add(new SharedPoint(new ControlPoint(new Vector3(5f, 0f, 0f))));

            SetupProperties();
            Refresh();
        }

        protected override void OnSave()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public override void DrawEditor()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                ShowCountField();

                EditorGUILayout.BeginVertical(Extensions.BoxedHeaderStyle);
                {
                    for (int i = 0; i < _controlPoints.Count; ++i)
                    {
                        _controlPoints[i].Set(_properties[i].Update());
                    }

                    //Vector3 p0 = EditorGUILayout.Vector3Field("P0", _curve.Start.Position);
                    //if (p0 != _curve.Start.Position)
                    //{
                    //    _curve.Start.Position = p0;
                    //}

                    //Vector3 p1 = EditorGUILayout.Vector3Field("P1", _curve.Start.Tangent);
                    //if (p1 != _curve.Start.Tangent)
                    //{
                    //    _curve.Start.Tangent = p1;
                    //}

                    //Vector3 p2 = EditorGUILayout.Vector3Field("P2", _curve.End.Tangent);
                    //if (p2 != _curve.End.Tangent)
                    //{
                    //    _curve.End.Tangent = p2;
                    //}

                    //Vector3 p3 = EditorGUILayout.Vector3Field("P3", _curve.End.Position);
                    //if (p3 != _curve.End.Position)
                    //{
                    //    _curve.End.Position = p3;
                    //}
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
            }

            EstablishHelper(useDefaultData);

            if (TargetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }

            UpdatePositions();
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (NeedsRefresh)
                {
                    Refresh();
                }
            }
        }

        private void UpdatePositions()
        {
            if (_createdObjects.Count > 0)
            {
                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    _createdObjects[i].transform.position = GetDefaultPositionAtIndex(i);
                }
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            float n = _createdObjects.Count - 1;
            float t = (float)index / n;
            return _curve.GetPointOnCurve(t);

        }

        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Quaternion targetRotation = _target.transform.rotation;
                GameObject clone = GameObject.Instantiate(_target, proxy.transform.position, targetRotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);
                
                _createdObjects.Add(clone);
            }
        }

        protected override void OnSceneGUI(SceneView view)
        {
            bool needsRefresh = false;
            ControlPoint start = _curve.Start;
            Vector3 p0 = Handles.PositionHandle(start.Position, Quaternion.identity);
            if (p0 != start.Position)
            {
                start.Position = p0;
                needsRefresh = true;
            }

            Vector3 p1 = Handles.PositionHandle(start.Tangent, Quaternion.identity);
            if (p1 != start.Tangent)
            {
                start.Tangent = p1;
                needsRefresh = true;
            }

            Handles.color = Color.cyan;
            Handles.DrawLine(start.Position, start.Tangent);
            Handles.SphereHandleCap(0, start.Tangent, Quaternion.identity, .25f, EventType.Repaint);

            ControlPoint end = _curve.End;
            Vector3 p2 = Handles.PositionHandle(end.Tangent, Quaternion.identity);
            if (p2 != end.Tangent)
            {
                end.Tangent = p2;
                needsRefresh = true;
            }

            Vector3 p3 = Handles.PositionHandle(end.Position, Quaternion.identity);
            if (p3 != end.Position)
            {
                end.Position = p3;
                needsRefresh = true;
            }

            Handles.DrawLine(end.Position, end.Tangent);
            Handles.SphereHandleCap(0, end.Tangent, Quaternion.identity, .25f, EventType.Repaint);
            Handles.DrawBezier(start.Position, end.Position, start.Tangent, end.Tangent, Color.cyan, null, 3f);

            if (needsRefresh)
            {
                Refresh();
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
                
                // #DG: add bezier specic mods here
                ModifierType.FollowCurve,
                ModifierType.IncrementalRotation,
                ModifierType.IncrementalScale,
                ModifierType.PositionNoise,
            };

            return mods;
        }

        protected override ArrayState GetContainerData()
        {
            return null;
        }

        protected override void PopulateFromExistingData(ArrayState data)
        {
            throw new System.NotImplementedException();
        }

        public override void OnStateSet(ArrayState stateData)
        {
            //
        }

        public void SetupProperties()
        {
            int i = 0;
            foreach (SharedPoint point in _controlPoints)
            {
                _properties.Add(BezierProperty.Create($"P{i}", point, CommandQueue));
                ++i;
            }
        }
    }
}