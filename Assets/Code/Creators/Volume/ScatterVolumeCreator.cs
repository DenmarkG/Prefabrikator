using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public abstract class ScatterVolumeCreator : ArrayCreator
    {
        public override float MaxWindowHeight => 300f;
        public override string Name => "Scatter";

        public override int MinCount => 1;
        protected static readonly int DefaultCount = 10;

        protected List<Vector3> _positions = new List<Vector3>();

        protected bool IsEditMode => _editMode != EditMode.None;
        protected EditMode _editMode = EditMode.None;

        protected SceneView _sceneView = null;

        protected bool IsDirty { get; private set; }
        
        protected Shared<Vector3> _center = new Shared<Vector3>();
        protected Vector3Property _centerProperty = null;

        public ScatterVolumeCreator(GameObject target)
            : base(target, DefaultCount)
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
                _positions.Clear();
            }

            EstablishHelper(useDefaultData);

            if (TargetCount != _createdObjects.Count)
            {
                OnTargetCountChanged();
            }
        }

        protected override void OnTargetCountChanged()
        {
            if (TargetCount < _createdObjects.Count)
            {
                while (_createdObjects.Count > TargetCount)
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
                while (TargetCount > _createdObjects.Count)
                {
                    CreateClone();
                }
            }
        }

        protected abstract void Scatter();

        protected void MarkDirty()
        {
            IsDirty = true;
        }

        protected abstract void UpdatePositions();

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
