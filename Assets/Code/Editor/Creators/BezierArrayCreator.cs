using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
#if SPLINE_CREATOR
    using SharedPoint = Shared<ControlPoint>;

    public class BezierArrayCreator : ArrayCreator
    {
        public enum BezierTangenet
        {
            None,
            Smooth,
            Broken,
        }

        private struct Point
        {
            public static readonly Point Default = new Point()
            {
                Tangent = BezierTangenet.None
            };

            public Vector3 Position;
            public BezierTangenet Tangent;

            public static implicit operator Vector3(Point p) => p.Position;
            public static explicit operator Point(Vector3 other) => new Point() { Position = other };
        }

        private static readonly int DefaultCount = 3;
        public override int MinCount => DefaultCount;

        public override float MaxWindowHeight => 300f;

        public override string Name => "Spline";

        public override ShapeType Shape => ShapeType.Spline;

        private List<Vector3Property> _properties = new List<Vector3Property>();

        private int _selectedPoint = -1;

        private List<Point> _points = new List<Point>()
        {
            new Point() { Position = new Vector3(), Tangent = BezierTangenet.Smooth },
            new Point() { Position = (Vector3.up * 4) },
            new Point() { Position = ((Vector3.up * 6) + (Vector3.right * 2)) },
            new Point() { Position = ((Vector3.up * 6) + (Vector3.right * 6)), Tangent = BezierTangenet.Smooth }
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
                    Point point = _points[i];
                    Point? prevPoint = null;
                    Point? nextPoint = null;

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            string label = (i == _selectedPoint) ? $">>> P{i}" : $"P{i}";
                            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(Extensions.LabelWidth + Extensions.IndentSize));

                            if (i == 0)
                            {
                                // #DG: special case #1
                                nextPoint = _points[i + 1];
                            }
                            else if (i == _points.Count - 1)
                            {
                                // #DG: special case #2
                                prevPoint = _points[i - 1];
                            }
                            else
                            {
                                prevPoint = _points[i - 1];
                                nextPoint = _points[i + 1];
                            }


                            if (i % 3 == 0)
                            {
                                Vector3 posBefore = point.Position;
                                point.Position = EditorGUILayout.Vector3Field(GUIContent.none, point);

                                if (point.Tangent == BezierTangenet.Smooth)
                                {
                                    Vector3 delta = posBefore - point.Position;
                                    if (prevPoint != null)
                                    {
                                        prevPoint = (Point)(prevPoint.Value.Position + (point.Position - delta));
                                    }

                                    if (nextPoint != null)
                                    {
                                        nextPoint = (Point)(nextPoint.Value.Position + (point.Position + delta));
                                    }
                                }

                                string tangent = point.Tangent.ToString();
                                if (GUILayout.Button(tangent, GUILayout.Width(Extensions.LabelWidth)))
                                {
                                    point.Tangent = (point.Tangent == BezierTangenet.Broken) ? BezierTangenet.Smooth : BezierTangenet.Broken;
                                }
                            }
                            else
                            {
                                point.Position = EditorGUILayout.Vector3Field(GUIContent.none, point);
                                GUILayout.Space(Extensions.LabelWidth + 3); // #DG: buttons have a hidden width
                            }
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            _points[i] = point;

                            if (i % 3 == 0)
                            {
                                if (prevPoint != null)
                                {
                                    _points[i - 1] = prevPoint.Value;
                                }
                                if (nextPoint != null)
                                {
                                    try
                                    {
                                        _points[i + 1] = nextPoint.Value;
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogException(e);
                                    }
                                }
                            }

                            if (_sceneView != null)
                            {
                                EditorUtility.SetDirty(_sceneView);
                            }
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
            _points.Add((Point)p1);

            Vector3 p2 = direction + p1;
            _points.Add((Point)p2);

            Vector3 p3 = direction + p2;
            _points.Add(new Point() { Position = p3, Tangent = BezierTangenet.Smooth });

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
            int startIndex = 0;
            if (t >= 1f)
            {
                t = 1f;
                startIndex = _points.Count - 4;
            }
            else
            {
                int segment = Mathf.FloorToInt(t * _numSegments);
                float tSubS = t * _numSegments;
                float tSubN = tSubS - segment;
                t = tSubN;
                startIndex = segment * 3;
            }
            Vector3 p0, p1, p2, p3;

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

                    if (i == _selectedPoint)
                    {
                        Vector3 position = Handles.PositionHandle(point, Quaternion.identity);
                        if (position != point)
                        {
                            _points[i] = (Point)position;
                            needsRefresh = needsRefresh || true;
                        }
                    }
                    else
                    {
                        const float size = .2f;
                        bool isControlPoint = (i == 0) || (i % 3 == 0);

                        if (Handles.Button(point, Quaternion.identity, size, size, isControlPoint ? Handles.CubeHandleCap : Handles.SphereHandleCap))
                        {
                            _selectedPoint = i;
                        }
                    }

                    // #DG: fix this. currently it doesn't link the right points
                    Handles.color = Color.white;
                    if ((i == 0) || (i % 3 == 0))
                    {
                        if (i - 1 > 0)
                        {
                            Handles.DrawLine(_points[i - 1], _points[i]);
                        }
                        
                        if ((i + 1) < _points.Count)
                        {
                            Handles.DrawLine(_points[i + 1], _points[i]);
                        }
                    }

                    if (i > 0)
                    {
                        if (i % 3 == 0)
                        {
                            Vector3 p0 = _points[i - 3];
                            Vector3 p1 = _points[i - 2];
                            Vector3 p2 = _points[i - 1];
                            Vector3 p3 = _points[i];
                            Handles.DrawBezier(p0, p3, p1, p2, Color.black, null, 3f);
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
#endif // SPLINE_CREATOR
}
