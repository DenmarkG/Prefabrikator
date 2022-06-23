using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class ArcArrayData : ArrayData
    {
        public float FillPercent = ArcArrayCreator.DefaultFillPercent;
        public bool CapEnd = false;
        public float Radius = CircularArrayCreator.DefaultRadius;

        public ArcArrayData(GameObject prefab, Quaternion targetRotation)
            : base(ShapeType.Arc, prefab, targetRotation)
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
            EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle);
            {
                EditorGUILayout.LabelField("Fill", GUILayout.Width(Extensions.LabelWidth));
                float fillPercent = EditorGUILayout.Slider(_fillPercent, 0f, .9999f, null);
                if (fillPercent != _fillPercent)
                {
                    _fillPercent = fillPercent;
                    //_needsRefresh = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            base.DrawEditor();
        }

        protected override void UpdatePositions()
        {
            for (int i = 0; i < _createdObjects.Count; ++i)
            {
                Vector3 position = GetDefaultPositionAtIndex(i);
                _createdObjects[i].transform.localPosition = position + _center;
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            float degrees = (360 * _fillPercent) * Mathf.Deg2Rad; // #DG: TODO multiply this by fill percent
            int n = _createdObjects.Count - 1;
            float angle = (n != 0f) ? (degrees / n) : 0f;

            float t = angle * index;
            float x = Mathf.Cos(t) * _radius;
            float z = Mathf.Sin(t) * _radius;

            return new Vector3(x, _target.transform.position.y, z);
        }

        protected override ArrayData GetContainerData()
        {
            ArcArrayData data = new ArcArrayData(_target, Quaternion.identity);
            data.Count = _targetCount;
            data.Radius = _radius;
            data.FillPercent = _fillPercent;

            return data;
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            if (data is ArcArrayData arcData)
            {
                _targetCount = arcData.Count;
                _radius.Set(arcData.Radius);
                _fillPercent = arcData.FillPercent;
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
                ModifierType.IncrementalRotation,
                ModifierType.IncrementalScale,
                ModifierType.PositionNoise,
                // #DG: add circle specic mods here
                ModifierType.FollowCurve,
            };

            return mods;
        }
    }
}
