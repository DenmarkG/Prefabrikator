using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class ScatterVolumeCreator : ArrayCreator
    {
        public override float MaxWindowHeight => 300f;

        private static readonly int MinCount = 3;

        private Vector3[] _positions = null;

        public ScatterVolumeCreator(GameObject target)
            : base(target, MinCount)
        {
            //
        }

        public override void DrawEditor()
        {
            if (GUILayout.Button("Scatter"))
            {
                Scatter();
            }
        }

        public override Vector3 GetDefaultPositionAtIndex(int index)
        {
            return _positions[index];
        }

        public override void UpdateEditor()
        {
            if (_target != null)
            {
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

        protected abstract void Scatter();

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
