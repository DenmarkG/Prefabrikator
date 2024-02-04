using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Prefabrikator.GridArrayCreator;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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

        private Shared<Axis> _axis = new(Axis.Z);
        private Shared<bool> _negateAxis = new(false);
        private BoolProperty _negateProperty = null;

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

            void OnNegateChanged(bool current, bool previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<bool>(_negateAxis, previous, current));
            }
            _negateProperty = new BoolProperty("Flip Axis", _negateAxis, OnNegateChanged);
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
            Axis axis = (Axis)EditorGUILayout.EnumPopup("Follow Axis", _axis);
            if (axis != _axis)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Axis>(_axis, _axis.Get(), axis));
            }

            _negateAxis.Set(_negateProperty.Update());
        }

        private void SetRoationFromCircle(TransformProxy[] proxies)
        {
            CircularArrayCreator circle = Owner as CircularArrayCreator;
            if (circle != null)
            {
                int numObjs = proxies.Length;
                Vector3 center = circle.Center;
                TransformProxy current;
                for (int i = 0; i < numObjs; ++i)
                {
                    current = proxies[i];

                    Vector3 position = current.Position;
                    Vector3 relativePosition = (position - center).normalized;
                    float directionScalar = _negateAxis.Get() ? -1 : 1;

                    if (Owner is SphereArrayCreator sphere)
                    {
                        

                        var (RotX, RotY) = sphere.GetRotationsAtIndex(i);

                        Vector3 upVector = Quaternion.Euler(RotX, 0f, 0f) * Vector3.up;
                        Vector3 rightVector = Quaternion.Euler(0f, RotY, 0f) * Vector3.right;

                        Vector3 upTangent = Vector3.Cross(relativePosition, upVector);
                        Vector3 rightTangent = Vector3.Cross(relativePosition, rightVector);

                        Quaternion targetRotation = _axis.Get() switch
                        {
                            Axis.X => Quaternion.LookRotation(-relativePosition * directionScalar),
                            Axis.Y => Quaternion.LookRotation(upVector, upTangent),
                            _ => Quaternion.LookRotation(upTangent * directionScalar)
                        };

                        current.Rotation = targetRotation;
                    }
                    else
                    {
                        
                        Vector3 cross = Vector3.Cross(relativePosition, circle.UpVector);

                        Quaternion targetRotation = _axis.Get() switch
                        {
                            Axis.X => Quaternion.LookRotation(-relativePosition * directionScalar),
                            Axis.Y => Quaternion.LookRotation(-circle.UpVector, cross),
                            _ => Quaternion.LookRotation(cross * directionScalar)
                        };

                        current.Rotation = targetRotation;
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
                    Vector3 relativePosition = (current.Position - ellipse.Center).normalized;

                    float directionScalar = _negateAxis.Get() ? -1 : 1;
                    Quaternion targetRotation = _axis.Get() switch
                    {
                        Axis.X => Quaternion.LookRotation(-relativePosition * directionScalar),
                        Axis.Y => Quaternion.LookRotation(-ellipse.UpVector, tangent),
                        _ => Quaternion.LookRotation(tangent)
                    };

                    current.Rotation = targetRotation;

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