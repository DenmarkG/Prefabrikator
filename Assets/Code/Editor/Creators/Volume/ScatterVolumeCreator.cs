using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public abstract class ScatterVolumeCreator : ArrayCreator
    {
        protected enum Dimension
        {
            Two,
            Three
        }

        protected const int MaxSamples = 30;
        protected const int MaxShapes = 150;

        public override float MaxWindowHeight => 300f;
        public override string Name => "Scatter";

        public override int MinCount => 1;
        protected static readonly int DefaultCount = 10;

        protected List<Vector3> _positions = new List<Vector3>();

        protected bool IsDirty { get; private set; }
        
        protected Shared<Vector3> _center = new Shared<Vector3>();
        protected Vector3Property _centerProperty = null;

        protected Shared<float> _scatterRadius = new Shared<float>(2f);
        protected FloatProperty _scatterRadiusProperty = null;

        public ScatterVolumeCreator(GameObject target)
            : base(target, DefaultCount)
        {
            //
        }
        
        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Vector3 position = GetRandomPointInBounds();

                GameObject clone = GameObject.Instantiate(Original, position, Original.transform.rotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

                _positions.Add(position);
                Clones.Add(clone);
            }
        }

        public override sealed void DrawEditor()
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.BeginHorizontal();
                {
                    _scatterRadius.Set(_scatterRadiusProperty.Update());
                    GUILayout.Space(Constants.IndentSize);
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
            if (Original != null)
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

            if (TargetCount != Clones.Count)
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

        protected void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions = ScatterPoisson();

            while (_positions.Count < Clones.Count)
            {
                _positions.Add(GetRandomPointInBounds());
            }

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                ApplyToAll((go, index) => { go.transform.position = _positions[index]; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
        }

        protected virtual List<Vector3> ScatterPoisson(Vector3? initialPosition = null)
        {
            List<Vector3> scatteredPoints = new();
            List<Vector3> activePoints = new(TargetCount);

            Vector3 initialSample = initialPosition ?? GetInitialPosition();

            activePoints.Add(initialSample);

            while (activePoints.Count > 0 && scatteredPoints.Count < TargetCount)
            {
                bool sampleFound = false;
                Vector3[] samplePoints = GenerateSampleSet(initialSample, _scatterRadius, 2f * _scatterRadius, GetDimension());
                foreach (Vector3 sample in samplePoints)
                {
                    Vector3 testPosition = sample + initialSample;

                    if (IsValidPoint(scatteredPoints, testPosition))
                    {
                        activePoints.Add(testPosition);
                        scatteredPoints.Add(testPosition);
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
        
        protected Vector3? GetRandomPoisson(Vector3? initialSample = null)
        {
            if (Clones.Count == 0)
            {
                return null;
            }

            foreach (GameObject activeObject in Clones)
            {
                initialSample ??= activeObject.transform.position;
                Vector3[] samplePoints = GenerateSampleSet(initialSample.Value, _scatterRadius, 2f * _scatterRadius, GetDimension());
                foreach (Vector3 sample in samplePoints)
                {
                    Vector3 testPosition = sample + initialSample.Value;
                    if (IsValidPoint(_positions, testPosition))
                    {
                        return testPosition;
                    }
                }
            }

            return null;
        }

        private void UpdatePositions()
        {
            int count = Clones.Count;

            for (int i = 0; i < Clones.Count; ++i)
            {
                Clones[i].transform.position = _positions[i];
            }
        }

        protected virtual Vector3[] GenerateSampleSet(Vector3 center, float minRadius, float maxRadius, Dimension dimension)
        {
            Vector3[] samples = new Vector3[MaxSamples];
            for (int i = 0; i < MaxSamples; ++i)
            {
                Vector3 direction;

                if (dimension == Dimension.Two)
                {
                    Vector2 random = Random.insideUnitCircle;
                    direction = new Vector3(random.x, 0f, random.y);
                }
                else
                {
                    direction = Random.insideUnitSphere;
                }
                direction *= Random.Range(minRadius, maxRadius);
                samples[i] = direction;
            }

            return samples;
        }

        protected virtual void SetupProperties()
        {
            void OnScatterRadiusChanged(float current, float previous)
            {
                CommandQueue.Enqueue(new GenericCommand<float>(_scatterRadius, previous, current));
            }
            _scatterRadiusProperty = new FloatProperty("Scatter Radius", _scatterRadius, OnScatterRadiusChanged);
        }

        protected override int EnforceValidCount(int count)
        {
            return Mathf.Clamp(count, MinCount, MaxShapes);
        }

        protected abstract Vector3 GetRandomPointInBounds();
        protected abstract bool IsValidPoint(List<Vector3> scatteredPoints, Vector3 testPoint);
        protected abstract Vector3 GetInitialPosition();
        protected abstract Dimension GetDimension();
    }
}
