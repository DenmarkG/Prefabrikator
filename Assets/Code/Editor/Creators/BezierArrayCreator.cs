using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RNG = UnityEngine.Random;

#if PATH
namespace Prefabrikator
{
    [System.Serializable]
    public class BezierArrayData : ArrayData
    {
        public CubicBezierCurve Curve = new CubicBezierCurve();
        public Vector3 EndRotation = new Vector3(0f, 90f, 0f);

        public BezierArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Path, prefab, targetRotation)
        {
            //
        }
    }

    public class BezierArrayCreator : ArrayCreator
    {
        private static readonly int DefaultCount = 3;
        public override int MinCount => DefaultCount;
        private bool _showControlPoints = false;

        public CubicBezierCurve Curve => _curve;
        private CubicBezierCurve _curve = new CubicBezierCurve();

        public override float MaxWindowHeight => 300f;

        public override string Name => "Path";

        public BezierArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
            SceneView.duringSceneGui += OnSceneGUI;

            Refresh();
        }

        protected override void OnSave()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                ShowCountField();

                EditorGUILayout.BeginVertical(Extensions.BoxedHeaderStyle);
                {
                    _showControlPoints = EditorGUILayout.Foldout(_showControlPoints, "Control Points");
                    if (_showControlPoints)
                    {
                        Vector3 p0 = EditorGUILayout.Vector3Field("P0", _curve.Start.Position);
                        if (p0 != _curve.Start.Position)
                        {
                            _curve.Start.Position = p0;
                        }

                        Vector3 p1 = EditorGUILayout.Vector3Field("P1", _curve.Start.Tangent);
                        if (p1 != _curve.Start.Tangent)
                        {
                            _curve.Start.Tangent = p1;
                        }

                        Vector3 p2 = EditorGUILayout.Vector3Field("P2", _curve.End.Tangent);
                        if (p2 != _curve.End.Tangent)
                        {
                            _curve.End.Tangent = p2;
                        }

                        Vector3 p3 = EditorGUILayout.Vector3Field("P3", _curve.End.Position);
                        if (p3 != _curve.End.Position)
                        {
                            _curve.End.Position = p3;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
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

        protected override ArrayData GetContainerData()
        {
            BezierArrayData data = new BezierArrayData(_target, _targetRotation);
            data.Count = TargetCount;
            data.Curve = _curve;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is BezierArrayData curveData)
            {
                SetTargetCount(curveData.Count);
                _targetRotation = curveData.TargetRotation;
                _curve = curveData.Curve;
            }
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
                    CreateClone();
                }
            }

            Refresh();
        }

        protected override void OnSceneGUI(SceneView view)
        {
            if (_showControlPoints)
            {
                bool needsRefresh = false;
                BezierPoint start = _curve.Start;
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

                BezierPoint end = _curve.End;
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
    }
}
#endif // PATH