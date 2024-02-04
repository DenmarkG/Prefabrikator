using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Graphs;

namespace Prefabrikator
{
    public class SphereArrayCreator : CircularArrayCreator
    {
        public struct RotationInfo
        {
            public float RotX;
            public float RotY;
            //public float Forward;

            public void Deconstruct(out float rotX, out float rotY)
            {
                rotX = RotX;
                rotY = RotY;
            }
        }

        public override ShapeType Shape => ShapeType.Sphere;
        public override float MaxWindowHeight => 350f;
        public override string Name => "Sphere";

        public static readonly int DefaultSectorCount = 16;
        private Shared<int> _sectorCount = new Shared<int>(DefaultSectorCount);

        public static readonly int DefaultStackCount = 8;
        private Shared<int> _stackCount = new Shared<int>(DefaultStackCount);

        private const float PiOverTwo = Mathf.PI / 2f;
        private const float TwoPi = Mathf.PI * 2f; // 360

        private List<Vector3> _defaultPositions = new List<Vector3>();
        private List<RotationInfo> _rotations = null;


        public SphereArrayCreator(GameObject target)
            : base(target)
        {
            SetTargetCount(GetTargetCount());
            _radius.Set(10f);
        }

        public override void DrawEditor()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.BeginVertical();
                {
                    _center.Set(_centerProperty.Update());
                    _radius.Set(Mathf.Abs(_radiusProperty.Update()));

                    int sectorCount = _sectorCount;
                    if (DisplayCountField(ref sectorCount, "Segments"))
                    {
                        sectorCount = Mathf.Max(sectorCount, MinCount);
                        CommandQueue.Enqueue(new GenericCommand<int>(_sectorCount, _sectorCount, sectorCount));
                    }

                    int stackCount = _stackCount;
                    if (DisplayCountField(ref stackCount, "Rings"))
                    {
                        stackCount = Mathf.Max(stackCount, MinCount);
                        CommandQueue.Enqueue(new GenericCommand<int>(_stackCount, _stackCount, stackCount));
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }


        // Algorithm is here: https://www.songho.ca/opengl/gl_sphere.html
        // Sector = y rotation
        // Stack = x rotation
        protected override void UpdatePositions()
        {
            if (TargetCount < MinCount || Clones.Count < MinCount)
            {
                return;
            }

            _rotations = new(new RotationInfo[Clones.Count]);

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
                Clones[0].transform.localPosition = position + _center;

                _rotations[0] = new RotationInfo() { RotX = stackAngle, RotY = sectorAngle };

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
                    Clones[index].transform.localPosition = position + _center;

                    _rotations[index] = new RotationInfo() { RotX = stackAngle, RotY = sectorAngle };

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
                Clones[Clones.Count - 1].transform.localPosition = position + _center;
                
                _rotations[Clones.Count - 1] = new RotationInfo() { RotX = stackAngle, RotY = sectorAngle };
            }

            int count = Clones.Count;
            if (_defaultPositions.Count != count)
            {
                _defaultPositions = new List<Vector3>();
                for (int i = 0; i < count; ++i)
                {
                    _defaultPositions.Add(Clones[i].transform.position);
                }
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    _defaultPositions[i] = (Clones[i].transform.position);
                }
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return _defaultPositions[index];
        }

        public RotationInfo GetRotationsAtIndex(int index)
        {
            return _rotations[index];
        }

        protected override sealed void VerifyTargetCount()
        {
            SetTargetCount(GetTargetCount());
            base.VerifyTargetCount();
        }

        private int GetTargetCount()
        {
            return ((_stackCount * _sectorCount) - _sectorCount) + 2; // #DG: +2 for end caps
        }

        public bool DisplayCountField(ref int targetCount, string label = null)
        {
            bool needsRefresh = false;

            EditorGUILayout.BeginHorizontal(Extensions.BoxedHeaderStyle, GUILayout.Width(Constants.MaxWidth));
            {
                EditorGUILayout.LabelField(label ?? "Count", GUILayout.Width(Constants.LabelWidth));

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    if (targetCount > 0)
                    {
                        --targetCount;
                        needsRefresh = true;
                    }
                }

                var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField(targetCount.ToString(), style, GUILayout.ExpandWidth(true), GUILayout.Width(Constants.LabelWidth));

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    if (targetCount < int.MaxValue - 1)
                    {
                        ++targetCount;
                        needsRefresh = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            return needsRefresh;
        }
    }
}
