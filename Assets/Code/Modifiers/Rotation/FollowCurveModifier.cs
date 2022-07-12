using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class FollowCurveModifier : Modifier, IRotator
    {
        private enum CurveMode
        {
            Circle,
            Path,
            Ellipse,
        }

        protected override string DisplayName => "Follow Curve";
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
            else if (owner is BezierArrayCreator)
            {
                _curveMode = CurveMode.Path;
            }
            else
            {
                Debug.LogError("Attempting to an invalid curve modifier");
            }
        }

        public override void OnRemoved()
        {
            Owner.ApplyToAll((go) => { go.transform.rotation = Owner.GetDefaultRotation(); });
        }

        public override void Process(GameObject[] objs)
        {
            if (_rotations == null || _rotations.Length != objs.Length)
            {
                _rotations = new Quaternion[objs.Length];
            }

            switch (_curveMode)
            {
                case CurveMode.Path:
                    SetRotationFromPath(objs);
                    break;
                case CurveMode.Ellipse:
                    SetRotationFromEllipse(objs);
                    break;
                case CurveMode.Circle:
                default:
                    SetRoationFromCircle(objs);
                    break;
            }
        }

        protected override void OnInspectorUpdate()
        {
            // #DG: Add follow axis? 
        }

        private void SetRoationFromCircle(GameObject[] objs)
        {
            CircularArrayCreator circle = Owner as CircularArrayCreator;
            if (circle != null)
            {
                bool isSphere = Owner is SphereArrayCreator;
                int numObjs = objs.Length;
                Vector3 center = circle.Center;
                GameObject current = null;
                for (int i = 0; i < numObjs; ++i)
                {
                    current = objs[i];
                    Vector3 position = current.transform.position;

                    if (isSphere)
                    {
                        current.transform.localRotation = Quaternion.LookRotation(center - position);
                    }
                    else
                    {
                        Vector3 cross = Vector3.Cross((position - center).normalized, circle.UpVector);
                        current.transform.localRotation = Quaternion.LookRotation(cross);
                    }

                    _rotations[i] = current.transform.localRotation;
                }
            }
        }

        private void SetRotationFromEllipse(GameObject[] objs)
        {
            EllipseArrayCreator ellipse = Owner as EllipseArrayCreator;
            if (ellipse != null)
            {
                int numObjs = objs.Length;
                GameObject current = null;
                int n = numObjs - 1;

                const float degrees = Mathf.PI * 2;
                float angle = (degrees / numObjs);

                for (int i = 0; i < numObjs; ++i)
                {
                    float t = angle * i;
                    current = objs[i];
                    float xTan = -(ellipse.XRadius * Mathf.Sin(t));
                    float zTan = ellipse.ZRadius * Mathf.Cos(t);
                    Vector3 tangent = new Vector3(xTan, 0f, zTan);
                    current.transform.localRotation = Quaternion.LookRotation(tangent);
                    _rotations[i] = current.transform.localRotation;
                }
            }
        }

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
                    Vector3 tangent = path.Curve.GetTangentToCurve(t);
                    Quaternion rotation = Quaternion.LookRotation(tangent);
                    objs[i].transform.localRotation = rotation;

                    _rotations[i] = rotation;
                }
            }
        }

        public Quaternion GetRotationAtIndex(int index)
        {
            return _rotations[index];
        }
    }
}