using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class SphereArrayData : ArrayData
    {
        public float Radius = CircularArrayCreator.DefaultRadius;
        public CircularArrayCreator.OrientationType Orientation = CircularArrayCreator.OrientationType.Original;
        public int SectorCount = SphereArrayCreator.DefaultSectorCount;
        public int StackCount = SphereArrayCreator.DefaultStackCount;

        public SphereArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Sphere, prefab, targetRotation)
        {
            //
        }
    }

    public class SphereArrayCreator : CircularArrayCreator
    {
        public override float MaxWindowHeight => 350f;
        public override string Name => "Sphere";

        public static readonly int DefaultSectorCount = 16;
        private int _sectorCount = DefaultSectorCount;
        private CountProperty _sectorCountProperty = null;

        public static readonly int DefaultStackCount = 8;
        private int _stackCount = DefaultStackCount;
        private CountProperty _stackCountProperty = null;

        private const float PiOverTwo = Mathf.PI / 2f;
        private const float TwoPi = Mathf.PI * 2f; // 360

        

        public SphereArrayCreator(GameObject target)
            : base(target)
        {
            _targetCount = GetTargetCount();
            _radius.Set(10f);
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                {
                    float radius = EditorGUILayout.FloatField("Radius", _radius);
                    if (radius != _radius)
                    {
                        _radius.Set(Mathf.Abs(radius));
                        //_needsRefresh = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
                {
                    OrientationType orientation = (OrientationType)EditorGUILayout.EnumPopup("Rotation", _orientation);
                    if (orientation != _orientation)
                    {
                        _orientation = orientation;
                        OnOrientationChanged();
                        //_needsRefresh = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (Extensions.DisplayCountField(ref _sectorCount, "Segments"))
                {
                    _sectorCount = Mathf.Max(_sectorCount, MinCount);
                    //_needsRefresh = true;
                }

                if (Extensions.DisplayCountField(ref _stackCount, "Rings"))
                {
                    _stackCount = Mathf.Max(_stackCount, MinCount);
                    //_needsRefresh = true;
                }

                _targetCount = GetTargetCount();
            }
            EditorGUILayout.EndVertical();
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

        protected override void UpdatePositions()
        {
            float sectorStep = Mathf.PI * 2 / _sectorCount;
            float stackStep = Mathf.PI / _stackCount;
            int index = 0;

            // Cap the top of the sphere
            {
                float stackAngle = Mathf.PI / 2;
                float rCosPhi = _radius * Mathf.Cos(stackAngle);
                float z = _radius * Mathf.Sin(stackAngle);

                float sectorAngle = 0;
                float x = rCosPhi * Mathf.Cos(sectorAngle);
                float y = rCosPhi * Mathf.Sin(sectorAngle);

                Vector3 position = new Vector3(x, y, z);
                _createdObjects[0].transform.localPosition = position + _center;

                if (_orientation == OrientationType.FollowCircle)
                {
                    _createdObjects[0].transform.localRotation = Quaternion.LookRotation(_center - position);
                }

                ++index;
            }

            for (int i = 1; i < _stackCount; ++i)
            {
                float stackAngle = Mathf.PI / 2 - i * stackStep;
                float rCosPhi = _radius * Mathf.Cos(stackAngle);
                float z = _radius * Mathf.Sin(stackAngle);

                for (int j = 0; j < _sectorCount; ++j)
                {
                    float sectorAngle = j * sectorStep;
                    float x = rCosPhi * Mathf.Cos(sectorAngle);
                    float y = rCosPhi * Mathf.Sin(sectorAngle);

                    Vector3 position = new Vector3(x, y, z);
                    _createdObjects[index].transform.localPosition = position + _center;

                    if (_orientation == OrientationType.FollowCircle)
                    {
                        _createdObjects[index].transform.localRotation = Quaternion.LookRotation(_center - position);
                    }

                    ++index;
                }
            }

            //Cap the bottom of the sphere
            {
                float stackAngle = Mathf.PI / 2 - _stackCount * stackStep;
                float rCosPhi = _radius * Mathf.Cos(stackAngle);
                float z = _radius * Mathf.Sin(stackAngle);

                float sectorAngle = 0;
                float x = rCosPhi * Mathf.Cos(sectorAngle);
                float y = rCosPhi * Mathf.Sin(sectorAngle);

                Vector3 position = new Vector3(x, y, z);
                _createdObjects[_createdObjects.Count - 1].transform.localPosition = position + _center;

                if (_orientation == OrientationType.FollowCircle)
                {
                    _createdObjects[_createdObjects.Count - 1].transform.localRotation = Quaternion.LookRotation(_center - position);
                }
            }
        }

        private int GetTargetCount()
        {
            return ((_stackCount * _sectorCount) - _sectorCount) + 2; // #DG: +2 for end caps
        }

        protected override ArrayData GetContainerData()
        {
            SphereArrayData data = new SphereArrayData(_target, _targetRotation);
            data.Count = _targetCount;
            data.Radius = _radius;
            data.Orientation = _orientation;

            data.StackCount = _stackCount;
            data.SectorCount = _sectorCount;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is SphereArrayData sphereData)
            {
                _targetCount = sphereData.Count;
                _radius.Set(sphereData.Radius);
                _orientation = sphereData.Orientation;
                _targetRotation = sphereData.TargetRotation;

                _stackCount = sphereData.StackCount;
                _sectorCount = sphereData.SectorCount;
            }
        }
    }
}
