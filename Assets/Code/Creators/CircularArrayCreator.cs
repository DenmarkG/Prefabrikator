using UnityEngine;
using UnityEditor;
using RNG = UnityEngine.Random;

namespace Prefabrikator
{
    // #DG: Refactor this to extract common data for derived classes
    public class CircleArrayData : ArrayData
    {
        public float Radius = CircularArrayCreator.DefaultRadius;
        public CircularArrayCreator.OrientationType Orientation = CircularArrayCreator.OrientationType.Original;

        public CircleArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Circle, prefab, targetRotation)
        {
            Count = CircularArrayCreator.MinCount;
        }
    }

    // #DG: TODO create object to act as center, 
    public class CircularArrayCreator : ArrayCreator
    {
        // rotate bool, make objects rotate to center, or point along circle direction
        public enum OrientationType
        {
            Original,
            FollowCircle,
            Random,
        }

        public override float MaxWindowHeight => 350f;
        public override string Name => "Circle";

        public static readonly float DefaultRadius = 5f;
        protected Shared<float> _radius = new Shared<float>(DefaultRadius);
        protected FloatProperty _radiusProperty = null;

        public Vector3 Center => _center;
        public Vector3 UpVector => _targetProxy?.transform.up ?? Vector3.up;
        protected Vector3 _center = Vector3.zero;

        protected OrientationType _orientation = OrientationType.Original;

        public static readonly int MinCount = 3;
        private static readonly int MinCirlceCount = 6;

        public CircularArrayCreator(GameObject target)
            : base(target)
        {
            _center = _target.transform.position;
            _targetCount = MinCirlceCount;

            void OnRadiusSet(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_radius, previous, current));
            }
            _radiusProperty = new FloatProperty("Radius", _radius, OnRadiusSet);
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                _radius.Set(Mathf.Abs(_radiusProperty.Update()));

                EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                {
                    // #DG: convert this to modifier
                    OrientationType orientation = (OrientationType)EditorGUILayout.EnumPopup("Rotation", _orientation);
                    if (orientation != _orientation)
                    {
                        _orientation = orientation;
                        OnOrientationChanged();
                    }
                }
                EditorGUILayout.EndHorizontal();

                int currentCount = _targetCount;
                if (Extensions.DisplayCountField(ref currentCount))
                {
                    CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, Mathf.Max(currentCount, MinCount)));
                    //_needsRefresh = true;
                }
            }
            EditorGUILayout.EndVertical();
        }

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
            }

            EstablishHelper();

            if (_targetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }

            UpdatePositions();

            if (_orientation == OrientationType.Original)
            {
                UpdateLocalRotations();
            }
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
            const float degrees = Mathf.PI * 2;
            float angle = (degrees / _createdObjects.Count);

            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                float t = angle * i;
                float x = Mathf.Cos(t) * _radius;
                Vector3 right = Mathf.Cos(t) * _radius * _targetProxy.transform.right;

                float z = Mathf.Sin(t) * _radius;
                Vector3 forward = Mathf.Sin(t) * _radius * _targetProxy.transform.forward;

                Vector3 position = new Vector3(x, _targetProxy.transform.position.y, z);
                //Vector3 position = right + forward;

                _createdObjects[i].transform.localPosition = position + _center;

                if (_orientation == OrientationType.FollowCircle)
                {
                    Vector3 cross = Vector3.Cross((position - _center).normalized, _targetProxy.transform.up);
                    _createdObjects[i].transform.rotation = Quaternion.LookRotation(cross);
                }
            }
        }

        protected void OnOrientationChanged()
        {
            switch (_orientation)
            {
                case OrientationType.Random:
                    RandomizeAllRotations();
                    break;
                case OrientationType.Original:
                    ResetAllRotations();
                    break;
                case OrientationType.FollowCircle:
                default:
                    // Do Nothing, this will be handled during the update loop
                    break;
            }
        }

        private void ResetAllRotations()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.localRotation = _targetProxy.transform.rotation;
            }
        }

        private void RandomizeAllRotations()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                _createdObjects[i].transform.localRotation = GetRandomRotation();
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

        protected override void CreateClone(int index = 0)
        {
            Quaternion targetRotation = _target.transform.rotation;
            if (_orientation == OrientationType.Random)
            {
                targetRotation = GetRandomRotation();
            }

            GameObject clone = GameObject.Instantiate(_target, _center, targetRotation);
            clone.SetActive(true);
            clone.transform.SetParent(_targetProxy.transform);

            _createdObjects.Add(clone);
        }

        protected override ArrayData GetContainerData()
        {
            CircleArrayData data = new CircleArrayData(_target, _targetRotation);
            data.Count = _targetCount;
            data.Radius = _radius;
            data.Orientation = _orientation;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is CircleArrayData circleData)
            {
                _targetCount = circleData.Count;
                _radius.Set(circleData.Radius);
                _orientation = circleData.Orientation;
                _targetRotation = circleData.TargetRotation;
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
                while (_targetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            Vector3 center = Handles.PositionHandle(_center, Quaternion.identity);
            if (center != _center)
            {
                _center = center;
            }
        }

        protected override string[] GetAllowedModifiers()
        {
            string[] mods =
            {
                ModifierType.RotationRandom.ToString(),
                ModifierType.ScaleRandom.ToString(),
                ModifierType.ScaleUniform.ToString(),
                ModifierType.RotationRandom.ToString(),
                ModifierType.RotationUniform.ToString(),
                // #DG: add circle specic mods here
                ModifierType.FollowCurve.ToString(),
            };

            return mods;
        }
    }
}
