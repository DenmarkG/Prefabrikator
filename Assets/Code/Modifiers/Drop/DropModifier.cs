using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    class DropModifier : Modifier
    {
        protected override string DisplayName => ModifierType.DropToFloor;

        private List<Vector3> _positions = null;

        private Shared<LayerMask> _layer = new Shared<LayerMask>(LayerMask.NameToLayer("Default"));
        private LayerMaskProperty _layerProperty = null;

        private Shared<float> _dropDistance = new Shared<float>(10f);
        private FloatProperty _dropDistanceProperty = null;

        private Shared<bool> _useCollider = new Shared<bool>(true);
        private ToggleProperty _colliderProperty = null;

        private Shared<float> _verticalOffset = new Shared<float>();
        private FloatProperty _offsetProperty = null;

        public DropModifier(ArrayCreator creator)
            : base(creator)
        {
            SetupProperties();

            _positions = new List<Vector3>(Owner.CreatedObjects.Count);
        }

        public override void OnRemoved()
        {
            Owner.ApplyToAll((go, index) => { go.transform.position = Owner.GetDefaultPositionAtIndex(index); });
        }

        public override void Process(GameObject[] objs)
        {
            //
        }

        protected override void OnInspectorUpdate()
        {
            _dropDistance.Set(_dropDistanceProperty.Update());
            _useCollider.Set(_colliderProperty.Update());

            if (_useCollider == false)
            {
                _verticalOffset.Set(_offsetProperty.Update());
            }

            _layer.Set(_layerProperty.Update());
            if (GUILayout.Button("Drop"))
            {
                Drop();
            }
        }

        private void Drop()
        {
            Vector3[] previous = _positions.ToArray();
            List<GameObject> createdObjs = Owner.CreatedObjects;
            _positions = new List<Vector3>();

            GameObject current = null;
            for (int i = 0; i < createdObjs.Count; ++i)
            {
                Vector3 start = Owner.GetDefaultPositionAtIndex(i);
                current = createdObjs[i];
                current.transform.position = start;
                Collider collider = current.GetComponent<Collider>();

                RaycastHit[] hits = Physics.RaycastAll(start, Vector3.down, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != collider)
                    {
                        float offset = _verticalOffset;
                        if (_useCollider && collider != null)
                        {
                            offset = current.transform.position.y - collider.bounds.min.y;
                        }

                        _positions.Add(hit.point + (Vector3.up * offset));
                        break;
                    }
                }
            }

            while (_positions.Count < Owner.CreatedObjects.Count)
            {
                int index = Owner.CreatedObjects.Count - (Owner.CreatedObjects.Count - _positions.Count);
                _positions.Add(Owner.CreatedObjects[index].transform.position);
            }

            void Apply(Vector3[] positions)
            {
                Owner.ApplyToAll((go, index) => { go.transform.position = _positions[index]; });
            }
            var valueChanged = new ValueChangedCommand<Vector3[]>(previous, _positions.ToArray(), Apply);
            Owner.CommandQueue.Enqueue(valueChanged);
        }

        private void SetupProperties()
        {
            void OnDropDistanceChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_dropDistance, previous, current));
            }
            _dropDistanceProperty = new FloatProperty("Drop Distance", _dropDistance, OnDropDistanceChanged);

            void OnLayerMaskChanged(LayerMask current, LayerMask previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<LayerMask>(_layer, previous, current));
            }
            _layerProperty = new LayerMaskProperty("LayerMask", _layer, OnLayerMaskChanged);

            void OnOffsetChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_verticalOffset, previous, current));
            }
            _offsetProperty = new FloatProperty("Vertical Offset", _verticalOffset, OnOffsetChanged);

            void OnUseColliderChanged(bool current, bool previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<bool>(_useCollider, previous, current));
            }
            _colliderProperty = new ToggleProperty("Use Collider for Offset", _useCollider, OnUseColliderChanged);
        }
    }
}
