using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Prefabrikator
{

    public class ArcArrayCreator : CircularArrayCreator
    {
        public override ShapeType Shape => ShapeType.Arc;
        public override float MaxWindowHeight => 400f;
        public override string Name => "Arc";

        // how much of circle to fill; makes arcs possible
        public static readonly float DefaultFillPercent = .375f;
        private Shared<float> _fillPercent = new Shared<float>(DefaultFillPercent);
        private FloatSlider _fillProperty = null;

        private ArcHandle _arcHandle = new ArcHandle();
        private SphereBoundsHandle _radiusHandle = new SphereBoundsHandle();

        public ArcArrayCreator(GameObject target)
            : base(target)
        {
            _arcHandle.SetColorWithoutRadiusHandle(Color.gray, .25f);

            _fillProperty = new FloatSlider("Fill", _fillPercent, OnSliderChange);
            _fillProperty.OnEditModeEnter += () => { _editMode |= EditMode.Angle; };
            _fillProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Angle; };
        }

        public override void DrawEditor()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Fill", GUILayout.Width(Constants.LabelWidth));
                    _fillPercent.Set(_fillProperty.Update());
                }
                EditorGUILayout.EndHorizontal();
            }

            base.DrawEditor();
        }

        protected override void UpdatePositions()
        {
            for (int i = 0; i < Clones.Count; ++i)
            {
                Vector3 position = GetDefaultPositionAtIndex(i);
                Clones[i].transform.localPosition = position + _center;
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            float degrees = (360f * _fillPercent) * Mathf.Deg2Rad;
            int n = Clones.Count - 1;
            float angle = (n != 0f) ? (degrees / n) : 0f;

            float t = angle * index;
            float x = Mathf.Cos(t) * _radius;
            float z = Mathf.Sin(t) * _radius;

            return new Vector3(x, Original.transform.position.y, z);
        }

        private void OnSliderChange(float current, float previous)
        {
            CommandQueue.Enqueue(new GenericCommand<float>(_fillPercent, previous, current));
        }

        protected override void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            if (IsEditMode)
            {
                Vector3 center = _center;
                EditorGUI.BeginChangeCheck();
                {
                    // Position Handle
                    if (_editMode.HasFlag(EditMode.Center))
                    {
                        center = Handles.PositionHandle(_center, Quaternion.identity);
                    }

                    // Radius Handle
                    _radiusHandle.center = center;
                    _radiusHandle.radius = _radius;
                    _radiusHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Z;

                    if (_editMode.HasFlag(EditMode.Size))
                    {
                        _radiusHandle.DrawHandle();
                    }

                    // Arc Handle
                    _arcHandle.angle = 360f * _fillPercent;
                    _arcHandle.radius = _radius;
                    _arcHandle.radiusHandleDrawFunction = Handles.CubeHandleCap;

                    Vector3 handleDirection = Vector3.right;
                    Vector3 handleNormal = Vector3.Cross(handleDirection, Vector3.forward);
                    Matrix4x4 handleMatrix = Matrix4x4.TRS(
                        _center,
                        Quaternion.LookRotation(handleDirection, handleNormal),
                        Vector3.one
                    );

                    using (new Handles.DrawingScope(handleMatrix))
                    {
                        if (_editMode.HasFlag(EditMode.Angle))
                        {
                            _arcHandle.DrawHandle();
                        }
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (center != _center)
                    {
                        _center.Set(center);
                    }

                    float fillPercent = _arcHandle.angle / 360f;
                    if (!Mathf.Approximately(_fillPercent, fillPercent))
                    {
                        _fillPercent.Set(fillPercent);
                    }

                    if (_radiusHandle.radius != _radius)
                    {
                        _radius.Set(_radiusHandle.radius);
                    }
                }
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
                ModifierType.DropToFloor,
                // #DG: add circle specic mods here
                ModifierType.FollowCurve,
            };

            return mods;
        }
    }
}
