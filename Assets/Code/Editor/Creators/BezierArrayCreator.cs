using System.Collections.Generic;
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

        public override string Name => "Path";

        public override ShapeType Shape => ShapeType.Path;

        private List<SharedPoint> _controlPoints = new List<SharedPoint>(2)
        {
            new SharedPoint(ControlPoint.Default),
            new SharedPoint(new ControlPoint(new Vector3(5f, 0f, 0f)))
        };
        private List<Vector3Property> _properties = new List<Vector3Property>();


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
            using (new EditorGUI.IndentLevelScope())
            {
                ShowCountField();
                for (int i = 0; i < _controlPoints.Count; ++i)
                {
                    ref ControlPoint point = ref _controlPoints[i].GetRef();
                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    {
                        EditorGUILayout.LabelField("Position", GUILayout.MaxWidth(Extensions.LabelWidth + Extensions.IndentSize));
                        point.Position = EditorGUILayout.Vector3Field(GUIContent.none, point.Position);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    {
                        EditorGUILayout.LabelField("Tangent", GUILayout.MaxWidth(Extensions.LabelWidth + Extensions.IndentSize));
                        point.Tangent = EditorGUILayout.Vector3Field(GUIContent.none, point.Tangent);
                    }
                    EditorGUILayout.EndHorizontal();
                }
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
            return CubicBezierCurve.GetPointOnCurve(_controlPoints[0], _controlPoints[1], t);

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
            if (_controlPoints.Count > 0)
            {
                bool needsRefresh = false;
                for (int i = 0; i < _controlPoints.Count; ++i)
                {
                    Handles.color = Color.cyan;
                    ref ControlPoint point = ref _controlPoints[i].GetRef();
                    Vector3 position = Handles.PositionHandle(point.Position, Quaternion.identity);
                    Vector3 tangent = Handles.PositionHandle(point.Tangent, Quaternion.identity);
                    if (position != point.Position || tangent != point.Tangent)
                    {
                        point.Position = position;
                        point.Tangent = tangent;
                        needsRefresh = needsRefresh || true;
                    }

                    Handles.color = Color.white;
                    Handles.DrawLine(position, tangent);
                    if (i > 0)
                    {
                        Handles.DrawLine(tangent, _controlPoints[i - 1].Get().Tangent);
                    }
                }

                ref ControlPoint start = ref _controlPoints[0].GetRef();
                ref ControlPoint end = ref _controlPoints[1].GetRef();

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
            return CubicBezierCurve.GetTangentToCurve(_controlPoints[0], _controlPoints[1], t);
        }
    }
}
