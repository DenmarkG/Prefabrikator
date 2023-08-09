using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    // #DG: TODO create object to act as center, 
    public class CircularArrayCreator : ArrayCreator, IRadial
    {
        public override ShapeType Shape => ShapeType.Circle;

        public override float MaxWindowHeight => 350f;
        public override string Name => "Circle";

        public static readonly float DefaultRadius = 5f;

        public float Radius => _radius;
        protected Shared<float> _radius = new Shared<float>(DefaultRadius);
        protected FloatProperty _radiusProperty = null;

        public Vector3 Center => _center;
        public Vector3 UpVector => GetProxy()?.transform.up ?? Vector3.up;
        protected Shared<Vector3> _center = new Shared<Vector3>(Vector3.zero);
        protected Vector3Property _centerProperty = null;

        public override int MinCount => 5;
        private static readonly int DefaultCount = 8;

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
            _radiusProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Size; };

            void OnCenterChanged(Vector3 current, Vector3 previous)
            {
                CommandQueue.Enqueue(new GenericCommand<Vector3>(_center, previous, current));
            }
            _centerProperty = new Vector3Property("Center", _center, OnCenterChanged);
            _centerProperty.OnEditModeEnter += () => { _editMode |= EditMode.Center; };
            _centerProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Center; };
        }

        public override void DrawEditor()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.BeginVertical();
                {
                    _center.Set(_centerProperty.Update());
                    _radius.Set(Mathf.Abs(_radiusProperty.Update()));
                }
                EditorGUILayout.EndVertical();
            }

            ShowCountField();

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

        protected virtual void VerifyTargetCount()
        {
            if (TargetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
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
                
                _radiusHandle.center = center;
                _radiusHandle.radius = _radius;
                _radiusHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Z;

                EditorGUI.BeginChangeCheck();
                {
                    if (_editMode.HasFlag(EditMode.Center))
                    {
                        center = Handles.PositionHandle(_center, Quaternion.identity);
                    }

                    if (_editMode.HasFlag(EditMode.Size))
                    {
                        _radiusHandle.DrawHandle();
                    }
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
                ModifierType.DropToFloor,
                // #DG: add circle specic mods here
                ModifierType.FollowCurve,
                ModifierType.RadialNoise,
            };

            return mods;
        }
    }
}
