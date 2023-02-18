using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public abstract class ScatterVolumeCreator : ArrayCreator
    {
        private const int MaxSamples = 30;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Scatter";

        public override int MinCount => 1;
        protected static readonly int DefaultCount = 10;

        protected List<Vector3> _positions = new List<Vector3>();

        protected bool IsDirty { get; private set; }
        
        protected Shared<Vector3> _center = new Shared<Vector3>();
        protected Vector3Property _centerProperty = null;

        protected Shared<float> _scatterRadius = new Shared<float>(2f);

        public ScatterVolumeCreator(GameObject target)
            : base(target, DefaultCount)
        {
            //
        }

        public override sealed void DrawEditor()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(Extensions.IndentSize);
                    if (GUILayout.Button("Scatter"))
                    {
                        Scatter();
                    }
                }
                EditorGUILayout.EndHorizontal();

                DrawVolumeEditor();
            }

            ShowCountField();
        }

        protected abstract void DrawVolumeEditor();

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return _positions[index];
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
                if (IsDirty)
                {
                    UpdatePositions();
                    IsDirty = false;
                }

                if (NeedsRefresh)
                {
                    Refresh();
                }
            }
        }

        protected override void OnRefreshStart(bool hardRefresh = false, bool useDefaultData = false)
        {
            if (hardRefresh)
            {
                DestroyAll();
                _positions.Clear();
            }

            EstablishHelper(useDefaultData);

            if (TargetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }
        }

        protected void MarkDirty()
        {
            IsDirty = true;
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
                ModifierType.PositionNoise,
            };

            return mods;
        }

        protected List<Vector3> ScatterPoisson(Vector3? initialPosition = null)
        {
            List<Vector3> scatteredPoints = new();

            List<Vector3> activePoints = new(TargetCount);

            // #DG: Make this a user controlled variable
            Vector3 initialSample = initialPosition ?? GetInitialPosition();
            

            activePoints.Add(initialSample);

            while (activePoints.Count > 0 && scatteredPoints.Count < TargetCount)
            {
                bool sampleFound = false;
                Vector3[] samplePoints = GenerateSampleSet(initialSample, _scatterRadius, 2f * _scatterRadius);
                foreach (Vector3 sample in samplePoints)
                {
                    Vector3 testPosition = sample + initialSample;

                    if (IsValidPoint(scatteredPoints, testPosition))
                    {
                        activePoints.Add(testPosition);
                        scatteredPoints.Add(testPosition);
                        ValidatePoint(testPosition);
                        sampleFound = true;
                        break;
                    }
                }

                if (!sampleFound)
                {
                    activePoints.Remove(initialSample);
                }

                if (activePoints.Count > 0)
                {
                    initialSample = activePoints[Random.Range(0, activePoints.Count)];
                }
            }

            return scatteredPoints;
        }


        protected Vector3[] GenerateSampleSet(Vector3 center, float minRadius, float maxRadius)
        {
            Vector3[] samples = new Vector3[MaxSamples];
            for (int i = 0; i < MaxSamples; ++i)
            {
                Vector2 random = Random.insideUnitCircle;
                Vector3 direction = new Vector3(random.x, 0f, random.y);
                direction *= Random.Range(minRadius, maxRadius);
                samples[i] = direction;
            }

            return samples;
        }

        protected abstract void Scatter();
        protected abstract void UpdatePositions();
        protected abstract Vector3 GetRandomPointInBounds();
        protected abstract Vector3? GetRandomPoisson();
        protected abstract bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint);
        protected abstract Vector3 GetInitialPosition();

        protected virtual void ValidatePoint(Vector3 point) { }
    }
}
