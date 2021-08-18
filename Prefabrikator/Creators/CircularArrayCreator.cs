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

        public CircleArrayData(GameObject prefab, Vector3 targetScale, Quaternion targetRotation)
            : base(ArrayType.Circle, prefab, targetScale, targetRotation)
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
        protected float _radius = DefaultRadius;

        protected Vector3 _center = Vector3.zero;

        protected OrientationType _orientation = OrientationType.Original;

        public static readonly int MinCount = 3;
        private static readonly int MinCirlceCount = 6;

        public CircularArrayCreator(GameObject target)
            : base(target)
        {
            _center = _target.transform.position;
            _targetCount = MinCirlceCount;
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
                {
                    float radius = EditorGUILayout.FloatField("Radius", _radius);
                    if (radius != _radius)
                    {
                        _radius = Mathf.Abs(radius);
                        _needsRefresh = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
                {
                    OrientationType orientation = (OrientationType)EditorGUILayout.EnumPopup("Rotation", _orientation);
                    if (orientation != _orientation)
                    {
                        _orientation = orientation;
                        OnOrientationChanged();
                        _needsRefresh = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (ArrayToolExtensions.DisplayCountField(ref _targetCount))
                {
                    _targetCount = Mathf.Max(_targetCount, MinCount);
                    _needsRefresh = true;
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void Refresh(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
            }

            EstablishHelper();

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

            UpdatePositions();

            if (_orientation == OrientationType.Original)
            {
                UpdateLocalRotations();
            }

            UpdateLocalScales();

            _needsRefresh = false;
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (_needsRefresh)
                {
                    Refresh();
                }
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
                float z = Mathf.Sin(t) * _radius;

                Vector3 position = new Vector3(x, _targetProxy.transform.position.y, z);

                _createdObjects[i].transform.localPosition = position + _center;

                if (_orientation == OrientationType.FollowCircle)
                {
                    Vector3 cross = Vector3.Cross(_center - position, _target.transform.up);
                    _createdObjects[i].transform.localRotation = Quaternion.LookRotation(cross);
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

        private void CreateClone()
        {
            Quaternion targetRotation = _target.transform.rotation;
            if (_orientation == OrientationType.Random)
            {
                targetRotation = GetRandomRotation();
            }

            GameObject clone = GameObject.Instantiate(_target, _center, targetRotation);
            clone.transform.SetParent(_targetProxy.transform);

            _createdObjects.Add(clone);
        }

        protected override ArrayData GetContainerData()
        {
            CircleArrayData data = new CircleArrayData(_target, _targetScale, _targetRotation);
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
                _radius = circleData.Radius;
                _orientation = circleData.Orientation;
                _targetScale = circleData.TargetScale;
                _targetRotation = circleData.TargetRotation;
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            Vector3 center = Handles.PositionHandle(_center, Quaternion.identity);
            if (center != _center)
            {
                _center = center;
                _needsRefresh = true;
            }
        }
    }
}
