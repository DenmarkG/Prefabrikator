using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{
    public class EllipseArrayCreator : CircularArrayCreator
    {
        private Shared<float> _zRadius = new Shared<float>(7.5f);
        private FloatProperty _zRadiusProperty = null;

        public EllipseArrayCreator(GameObject target) 
            : base(target)
        {
            void OnZRadiusSet(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_zRadius, previous, current));
            }
            _zRadiusProperty = new FloatProperty("Z Radius", _zRadius, OnZRadiusSet);
        }

        public override void DrawEditor()
        {
            EditorGUILayout.BeginVertical();
            {
                _center.Set(_centerProperty.Update());
                _radius.Set(Mathf.Abs(_radiusProperty.Update()));
                _zRadius.Set(Mathf.Abs(_zRadiusProperty.Update()));

                int currentCount = _targetCount;
                if (Extensions.DisplayCountField(ref currentCount))
                {
                    CommandQueue.Enqueue(new CountChangeCommand(this, _createdObjects.Count, Mathf.Max(currentCount, MinCount)));
                }
            }
            EditorGUILayout.EndVertical();

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            GameObject proxy = GetProxy();

            const float degrees = Mathf.PI * 2;
            float angle = (degrees / _createdObjects.Count);

            float t = angle * index;
            float x = Mathf.Cos(t) * _radius;
            float z = Mathf.Sin(t) * _zRadius;

            return new Vector3(x, proxy.transform.position.y, z) + _center;
        }
    }
}

