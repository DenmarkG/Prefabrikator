using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class FollowCurveModifier : Modifier
    {
        private enum CurveMode
        {
            Circle,
            Path,
            Ellipse,
        }

        protected override string DisplayName => ModifierType.FollowCurve;
        private CurveMode _curveMode = CurveMode.Circle;

        private Quaternion[] _rotations = null;

        public FollowCurveModifier(ArrayCreator owner)
            : base(owner)
        {
            if (owner is EllipseArrayCreator)
            {
                _curveMode = CurveMode.Ellipse;
            }
            else if (owner is CircularArrayCreator)
            {
                _curveMode = CurveMode.Circle;
            }
#if SPLINE_CREATOR
            else if (owner is BezierArrayCreator)
            {
                _curveMode = CurveMode.Path;
            }
#endif
            else
            {
                Debug.LogError("Attempting to an invalid curve modifier");
            }
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override TransformProxy[] Process(TransformProxy[] proxies)
        {
            if (_rotations == null || _rotations.Length != proxies.Length)
            {
                _rotations = new Quaternion[proxies.Length];
            }

            switch (_curveMode)
            {
#if SPLINE_CREATOR
                case CurveMode.Path:
                    SetRotationFromPath(objs);
                    break;
#endif
                case CurveMode.Ellipse:
                    SetRotationFromEllipse(proxies);
                    break;
                case CurveMode.Circle:
                default:
                    SetRoationFromCircle(proxies);
                    break;
            }

            return proxies;
        }

        protected override void OnInspectorUpdate()
        {
            // #DG: Add follow axis? 
            GUILayout.BeginHorizontal(Extensions.LogStyle);
            {
                GUILayout.Label("This modifier has no editable options");
            }
            GUILayout.EndHorizontal();
        }

        private void SetRoationFromCircle(TransformProxy[] proxies)
        {
            CircularArrayCreator circle = Owner as CircularArrayCreator;
            if (circle != null)
            {
                bool isSphere = Owner is SphereArrayCreator;
                int numObjs = proxies.Length;
                Vector3 center = circle.Center;
                TransformProxy current;
                for (int i = 0; i < numObjs; ++i)
                {
                    current = proxies[i];
                    Vector3 position = current.Position;

                    if (isSphere)
                    {
                        current.Rotation = Quaternion.LookRotation(center - position);
                    }
                    else
                    {
                        Vector3 cross = Vector3.Cross((position - center).normalized, circle.UpVector);
                        current.Rotation = Quaternion.LookRotation(cross);
                    }

                    proxies[i] = current;
                    _rotations[i] = current.Rotation;
                }
            }
        }

        private void SetRotationFromEllipse(TransformProxy[] proxies)
        {
            EllipseArrayCreator ellipse = Owner as EllipseArrayCreator;
            if (ellipse != null)
            {
                int numObjs = proxies.Length;
                TransformProxy current;
                int n = numObjs - 1;

                const float degrees = Mathf.PI * 2;
                float angle = (degrees / numObjs);

                for (int i = 0; i < numObjs; ++i)
                {
                    float t = angle * i;
                    current = proxies[i];
                    float xTan = -(ellipse.XRadius * Mathf.Sin(t));
                    float zTan = ellipse.ZRadius * Mathf.Cos(t);
                    Vector3 tangent = new Vector3(xTan, 0f, zTan);
                    current.Rotation = Quaternion.LookRotation(tangent);

                    proxies[i] = current;
                    _rotations[i] = current.Rotation;
                }
            }
        }

#if SPLINE_CREATOR
        private void SetRotationFromPath(GameObject[] objs)
        {
            BezierArrayCreator path = Owner as BezierArrayCreator;

            if (path != null)
            {
                float n = objs.Length - 1;
                int numObjs = objs.Length;

                for (int i = 0; i < numObjs; ++i)
                {
                    float t = (float)i / n;
                    Vector3 tangent = path.GetTangentToCurve(t);
                    Quaternion rotation = Quaternion.LookRotation(tangent);
                    objs[i].transform.localRotation = rotation;

                    _rotations[i] = rotation;
                }
            }
        }
#endif // SPLINE_CREATOR

        public Quaternion GetRotationAtIndex(int index)
        {
            return _rotations[index];
        }

        public override void Teardown()
        {
            Owner.ApplyToAll((go) => { go.transform.rotation = Owner.GetDefaultRotation(); });
        }
    }
}