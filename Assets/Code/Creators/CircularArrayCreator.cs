using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    // #DG: Refactor this to extract common data for derived classes
    public class CircleArrayData : ArrayData
    {
        public float Radius = CircularArrayCreator.DefaultRadius;

        public CircleArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Circle, prefab, targetRotation)
        {
            Count = CircularArrayCreator.MinCount;
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
        protected Vector3 _center = Vector3.zero;

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

                int currentCount = _targetCount;
                if (Extensions.DisplayCountField(ref currentCount))
                {
                    CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, Mathf.Max(currentCount, MinCount)));
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
            const float degrees = Mathf.PI * 2;
            float angle = (degrees / _createdObjects.Count);

            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                for (int i = 0; i < _createdObjects.Count; ++i)
                {
                    float t = angle * i;
                    float x = Mathf.Cos(t) * _radius;
                    Vector3 right = Mathf.Cos(t) * _radius * proxy.transform.right;

                    float z = Mathf.Sin(t) * _radius;
                    Vector3 forward = Mathf.Sin(t) * _radius * proxy.transform.forward;

                    Vector3 position = new Vector3(x, proxy.transform.position.y, z);
                    //Vector3 position = right + forward;

                    _createdObjects[i].transform.localPosition = position + _center;
                }
            }
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
            data.Count = _targetCount;
            data.Radius = _radius;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is CircleArrayData circleData)
            {
                _targetCount = circleData.Count;
                _radius.Set(circleData.Radius);
            }
        }

        protected virtual void VerifyTargetCount()
        {
            if (_targetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }
        }

        protected override sealed void OnTargetCountChanged()
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
