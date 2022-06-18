using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RNG = UnityEngine.Random;

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
        private static readonly int MinCount = 3;
        private bool _showControlPoints = false;

        public CubicBezierCurve Curve => _curve;
        private CubicBezierCurve _curve = new CubicBezierCurve();

        //

        public BezierArrayCreator(GameObject target)
            : base(target, MinCount)
        {
            SceneView.duringSceneGui += OnSceneGUI;

            Refresh();
        }

        ~BezierArrayCreator()
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

        public override float MaxWindowHeight => 300f;

        public override string Name => "Path";

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                int currentCount = _targetCount;
                if (Extensions.DisplayCountField(ref currentCount))
                {
                    currentCount = Mathf.Max(currentCount, MinCount);
                    CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, currentCount));
                }

                EditorGUILayout.BeginVertical(Extensions.BoxedHeaderStyle);
                {
                    _showControlPoints = EditorGUILayout.Foldout(_showControlPoints, "Control Points");
                    if (_showControlPoints)
                    {
                        Vector3 p0 = EditorGUILayout.Vector3Field("P0", _curve.Start.Position);
                        if (p0 != _curve.Start.Position)
                        {
                            _curve.Start.Position = p0;
                            //_needsRefresh = true;
                        }

                        Vector3 p1 = EditorGUILayout.Vector3Field("P1", _curve.Start.Tangent);
                        if (p1 != _curve.Start.Tangent)
                        {
                            _curve.Start.Tangent = p1;
                            //_needsRefresh = true;
                        }

                        Vector3 p2 = EditorGUILayout.Vector3Field("P2", _curve.End.Tangent);
                        if (p2 != _curve.End.Tangent)
                        {
                            _curve.End.Tangent = p2;
                            //_needsRefresh = true;
                        }

                        Vector3 p3 = EditorGUILayout.Vector3Field("P3", _curve.End.Position);
                        if (p3 != _curve.End.Position)
                        {
                            _curve.End.Position = p3;
                            //_needsRefresh = true;
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

            if (_targetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }

            UpdatePositions();

            //if (_orientation == OrientationType.Original)
            //{
            //    UpdateLocalRotations();
            //}
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
                float n = _createdObjects.Count - 1;
                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    float t = (float)i / n;
                    Vector3 position = _curve.GetPointOnCurve(t);
                    _createdObjects[i].transform.position = position;
                }
            }
        }

        protected override void CreateClone(int index)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Quaternion targetRotation = _target.transform.rotation;

                float t = (float)index / (float)_targetCount;
                Vector3 pointOnCurve = _curve.GetPointOnCurve(t);

                GameObject clone = GameObject.Instantiate(_target, pointOnCurve, targetRotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

                _createdObjects.Add(clone);
            }
        }

        private Quaternion GetRandomRotation()
        {
            float max = 360f;
            float min = -360;
            float x = RNG.Range(min, max);
            float y = RNG.Range(min, max);
            float z = RNG.Range(min, max);

            return Quaternion.Euler(new Vector3(x, y, z));
        }

        private void ResetAllRotations()
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    _createdObjects[i].transform.localRotation = proxy.transform.rotation;
                }
            }
        }

        private void RandomizeAllRotations()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.localRotation = GetRandomRotation();
            }
        }

        protected override ArrayData GetContainerData()
        {
            BezierArrayData data = new BezierArrayData(_target, _targetRotation);
            data.Count = _targetCount;
            data.Curve = _curve;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is BezierArrayData curveData)
            {
                _targetCount = curveData.Count;
                _targetRotation = curveData.TargetRotation;
                _curve = curveData.Curve;
            }
        }

        protected override void OnTargetCountChanged()
        {
            if (_targetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > _targetCount)
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
                int index = 0;
                while (_targetCount > _createdObjects.Count)
                {
                    CreateClone(index);
                    ++index;
                }
            }
        }

        private void OnSceneGUI(SceneView view)
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
