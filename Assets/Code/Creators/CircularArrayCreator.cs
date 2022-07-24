using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    // #DG: Refactor this to extract common data for derived classes
    public class CircleArrayData : ArrayData
    {
        public float Radius = CircularArrayCreator.DefaultRadius;

        public CircleArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Circle, prefab, targetRotation)
        {
            //Count = CircularArrayCreator.MinCount;
        }
    }

    // #DG: TODO create object to act as center, 
    public class CircularArrayCreator : ArrayCreator
    {
        public override float MaxWindowHeight => 350f;
        public override string Name => "Circle";

        public static readonly float DefaultRadius = 5f;
        protected Shared<float> _radius = new Shared<float>(DefaultRadius);
        protected FloatProperty _radiusProperty = null;

        public Vector3 Center => _center;
        public Vector3 UpVector => GetProxy()?.transform.up ?? Vector3.up;
        protected Shared<Vector3> _center = new Shared<Vector3>(Vector3.zero);
        protected Vector3Property _centerProperty = null;


        public override int MinCount => 5;
        private static readonly int DefaultCount = 6;

        protected SceneView _sceneView = null;

        protected bool IsEditMode => _editMode != EditMode.None;
        private EditMode _editMode = EditMode.None;

        private SphereBoundsHandle _radiusHandle = new SphereBoundsHandle();

        public CircularArrayCreator(GameObject target)
            : base(target, DefaultCount)
        {
            _center.Set(_target.transform.position);

            void OnRadiusSet(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_radius, previous, current));
            }
            _radiusProperty = new FloatProperty("Radius", _radius, OnRadiusSet);
            _radiusProperty.OnEditModeEnter += () => { _editMode |= EditMode.Size; };
            _radiusProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Size; };

            void OnCenterChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_center, previous, current));
            }
            _centerProperty = new Vector3Property("Center", _center, OnCenterChanged);
            _centerProperty.OnEditModeEnter += () => { _editMode |= EditMode.Center; };
            _centerProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Center; };

            SceneView.duringSceneGui += OnSceneGUI;
        }

        ~CircularArrayCreator()
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

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                _center.Set(_centerProperty.Update());
                _radius.Set(Mathf.Abs(_radiusProperty.Update()));

                ShowCountField();
            }
            EditorGUILayout.EndVertical();

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
            }

            EstablishHelper();

            VerifyTargetCount();

            UpdatePositions();
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                Refresh();
            }
        }

        protected virtual void UpdatePositions()
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    _createdObjects[i].transform.localPosition = GetDefaultPositionAtIndex(i);
                }
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            GameObject proxy = GetProxy();

            const float degrees = Mathf.PI * 2;
            float angle = (degrees / _createdObjects.Count);

            float t = angle * index;
            float x = Mathf.Cos(t) * _radius;
            float z = Mathf.Sin(t) * _radius;
            
            return new Vector3(x, proxy.transform.position.y, z) + _center;
        }

        protected override void CreateClone(int index = 0)
        {
            Quaternion targetRotation = _target.transform.rotation;

            GameObject clone = GameObject.Instantiate(_target, _center, targetRotation);
            clone.SetActive(true);
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                clone.transform.SetParent(proxy.transform);
            }

            _createdObjects.Add(clone);
        }

        protected override ArrayData GetContainerData()
        {
            CircleArrayData data = new CircleArrayData(_target, Quaternion.identity);
            data.Count = TargetCount;
            data.Radius = _radius;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is CircleArrayData circleData)
            {
                SetTargetCount(circleData.Count);
                _radius.Set(circleData.Radius);
            }
        }

        protected virtual void VerifyTargetCount()
        {
            if (TargetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }
        }

        protected override sealed void OnTargetCountChanged()
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
        }

        protected virtual void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            if (IsEditMode)
            {
                Vector3 center = _center;

                
                _radiusHandle.center = center;
                _radiusHandle.radius = _radius;
                _radiusHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Z;

                EditorGUI.BeginChangeCheck();
                {
                    center = Handles.PositionHandle(_center, Quaternion.identity);
                    _radiusHandle.DrawHandle();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (center != _center)
                    {
                        _center.Set(center);
                    }

                    if (_radiusHandle.radius != _radius)
                    {
                        _radius.Set(_radiusHandle.radius);
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
                ModifierType.PositionNoise,
                // #DG: add circle specic mods here
                ModifierType.FollowCurve,
            };

            return mods;
        }
    }
}
