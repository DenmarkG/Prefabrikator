using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public abstract class ScatterVolumeCreator : ArrayCreator
    {
        [System.Flags]
        protected enum EditMode : int
        {
            None = 0,
            Center = 0x1,
            Size = 0x2,
        }

        public override float MaxWindowHeight => 300f;
        public override string Name => "Scatter";

        protected static readonly int MinCount = 10;

        protected List<Vector3> _positions = new List<Vector3>();

        protected bool IsEditMode => _editMode != EditMode.None;
        protected EditMode _editMode = EditMode.None;

        protected SceneView _sceneView = null;

        protected bool IsDirty { get; private set; }

        
        protected Shared<Vector3> _center = new Shared<Vector3>();
        protected Vector3Property _centerProperty = null;

        public ScatterVolumeCreator(GameObject target)
            : base(target, MinCount)
        {
            //
        }

        public override sealed void DrawEditor()
        {
            if (GUILayout.Button("Scatter"))
            {
                Scatter();
            }

            DrawVolumeEditor();
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
            }

            EstablishHelper(useDefaultData);

            if (_targetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }
        }

        protected override void OnTargetCountChanged()
        {
            if (_targetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > _targetCount)
                {
                    int index = _createdObjects.Count - 1;
                    if (index >= 0)
                    {
                        DestroyClone(_createdObjects[_createdObjects.Count - 1]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                while (_targetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }
        }

        private void Scatter()
        {
            Vector3[] previous = _positions.ToArray();
            _positions.Clear();

            int count = _createdObjects.Count;

            for (int i = 0; i < count; ++i)
            {
                Vector3 position = GetRandomPointInBounds();
                _createdObjects[i].transform.position = position;
                _positions.Add(position);
            }

            void Apply(Vector3[] positions)
            {
                _positions = new List<Vector3>(positions);
                int count = positions.Length;
                ApplyToAll((go, index) => { go.transform.position = _positions[index]; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            CommandQueue.Enqueue(valueChanged);
        }

        protected void MarkDirty()
        {
            IsDirty = true;
        }

        protected virtual void UpdatePositions()
        {
            int count = _createdObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                _createdObjects[i].transform.position = _positions[i] + _center;
            }
        }

        protected abstract Vector3 GetRandomPointInBounds();

        protected override string[] GetAllowedModifiers()
        {
            string[] mods =
            {
                ModifierType.RotationRandom,
                ModifierType.ScaleRandom,
                ModifierType.ScaleUniform,
                ModifierType.RotationRandom,
                ModifierType.RotationUniform,
            };

            return mods;
        }
    }
}
