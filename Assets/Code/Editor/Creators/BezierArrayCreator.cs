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

        public override float MaxWindowHeight => 300f;

        public override string Name => "Spline";

        public override ShapeType Shape => ShapeType.Spline;

        private List<Vector3Property> _properties = new List<Vector3Property>();

        private List<Vector3> _points = new List<Vector3>()
        {
            new Vector3(),
            Vector3.up * 5,
            (Vector3.up * 5) + (Vector3.right * 5),
            Vector3.right * 5
        };

        private int _numSegments = 1;

        public BezierArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
            _refreshOnCountChange = true;
            
            SetupProperties();
            Refresh();
        }

        protected override void OnSave()
        {
            SceneView.RepaintAll();
        }

        public override void DrawEditor()
        {
            ShowCountField();
            using (new EditorGUI.IndentLevelScope())
            {
                for (int i = 0; i < _points.Count; ++i)
                {
                    Vector3 point = _points[i];
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUILayout.LabelField($"P{i}", GUILayout.MaxWidth(Extensions.LabelWidth + Extensions.IndentSize));
                            point = EditorGUILayout.Vector3Field(GUIContent.none, point);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            _points[i] = point;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(Extensions.IndentSize);
                if (GUILayout.Button("Add Point"))
                {
                    AddSegment();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddSegment()
        {
            // #DG: add to command queue
            int numPoints = _points.Count;
            Vector3 lastPoint = _points[numPoints - 1];

            Vector3 direction = lastPoint - _points[numPoints - 2];
            Vector3 p1 = direction + lastPoint;
            _points.Add(p1);

            Vector3 p2 = direction + p1;
            _points.Add(p2);

            Vector3 p3 = direction + p2;
            _points.Add(p3);

            ++_numSegments;
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
            //return CubicBezierCurve.GetPointOnCurve(_controlPoints[0], _controlPoints[1], t);

            ControlPoint start = new ControlPoint(_points[0], _points[1]);
            ControlPoint end = new ControlPoint(_points[3], _points[2]);

            Vector3 p0, p1, p2, p3;
            int startIndex = 0;

            if (t >= 1f)
            {
                t = 1f;
                startIndex = _points.Count - 4;
            }
            else 
            {
                int segment = (int)(t * _numSegments);
                startIndex = (segment * 3);
            }

            p0 = _points[startIndex];
            p1 = _points[startIndex + 1];
            p2 = _points[startIndex + 2];
            p3 = _points[startIndex + 3];

            return CubicBezierCurve.GetPointOnCurve(p0, p1, p2, p3, t);

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
            if (_points.Count > 0)
            {
                bool needsRefresh = false;
                for (int i = 0; i < _points.Count; ++i)
                {
                    Handles.color = Color.cyan;
                    Vector3 point = _points[i];
                    Vector3 position = Handles.PositionHandle(point, Quaternion.identity);
                    if (position != point)
                    {
                        _points[i] = position;
                        needsRefresh = needsRefresh || true;
                        Debug.Log("needs refesh");
                    }

                    // #DG: fix this. currently it doesn't link the right points
                    //Handles.color = Color.white;
                    //if (i % 2 != 0)
                    //{
                    //    if (i > 0)
                    //    {
                    //        Handles.DrawLine(_points[i - 1], _points[i]);
                    //    }
                    //}
                    
                    if (i > 0)
                    {
                        if (i % 3 == 0)
                        {
                            Vector3 p0 = _points[i - 3];
                            Vector3 p1 = _points[i - 2];
                            Vector3 p2 = _points[i - 1];
                            Vector3 p3 = _points[i];
                            Handles.DrawBezier(p0, p3, p1, p2, Color.cyan, null, 3f);
                        }
                    }
                }

                if (needsRefresh)
                {
                    Refresh();
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
            //
        }

        public Vector3 GetTangentToCurve(float t)
        {
            return default;
            //return CubicBezierCurve.GetTangentToCurve(_controlPoints[0], _controlPoints[1], t);
        }
    }
}
