using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class ArcArrayData : ArrayData
    {
        public float FillPercent = ArcArrayCreator.DefaultFillPercent;
        public bool CapEnd = false;
        public float Radius = CircularArrayCreator.DefaultRadius;
        public CircularArrayCreator.OrientationType Orientation = CircularArrayCreator.OrientationType.Original;

        public ArcArrayData(GameObject prefab, Vector3 targetScale, Quaternion targetRotation)
            : base(ArrayType.Arc, prefab, targetScale, targetRotation)
        {
            Count = CircularArrayCreator.MinCount;
        }
    }

    public class ArcArrayCreator : CircularArrayCreator
    {
        public override float MaxWindowHeight => 400f;
        public override string Name => "Arc";

        // how much of circle to fill; makes arcs possible
        public static readonly float DefaultFillPercent = .5f;
        private float _fillPercent = DefaultFillPercent;

        public ArcArrayCreator(GameObject target)
            : base(target)
        {
            //
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginHorizontal(_boxedHeaderStyle);
            {
                EditorGUILayout.LabelField("Fill", GUILayout.Width(Extensions.LabelWidth));
                float fillPercent = EditorGUILayout.Slider(_fillPercent, 0f, .9999f, null);
                if (fillPercent != _fillPercent)
                {
                    _fillPercent = fillPercent;
                    _needsRefresh = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            base.DrawEditor();
        }

        protected override void UpdatePositions()
        {
            float degrees = (360 * _fillPercent) * Mathf.Deg2Rad; // #DG: TODO multiply this by fill percent
            int n = _createdObjects.Count - 1;
            float angle = (degrees / n);

            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                float t = angle * i;
                float x = Mathf.Cos(t) * _radius;
                float z = Mathf.Sin(t) * _radius;

                Vector3 position = new Vector3(x, _target.transform.position.y, z);

                _createdObjects[i].transform.localPosition = position + _center;

                if (_orientation == OrientationType.FollowCircle)
                {
                    Vector3 cross = Vector3.Cross(_center - position, Vector3.up);
                    _createdObjects[i].transform.localRotation = Quaternion.LookRotation(cross);
                }
            }
        }

        protected override ArrayData GetContainerData()
        {
            ArcArrayData data = new ArcArrayData(_target, _targetScale, _targetRotation);
            data.Count = _targetCount;
            data.Radius = _radius;
            data.Orientation = _orientation;
            data.FillPercent = _fillPercent;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is ArcArrayData arcData)
            {
                _targetCount = arcData.Count;
                _radius = arcData.Radius;
                _orientation = arcData.Orientation;
                _fillPercent = arcData.FillPercent;
                _targetScale = arcData.TargetScale;
                _targetRotation = arcData.TargetRotation;
            }
        }
    }
}
