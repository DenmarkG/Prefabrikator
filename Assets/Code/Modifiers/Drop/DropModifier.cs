using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    class DropModifier : Modifier
    {
        protected override string DisplayName => "Drop To Floor";

        private Vector3[] _positions = null;

        private Shared<LayerMask> _layer = new Shared<LayerMask>(LayerMask.NameToLayer("Default"));
        private LayerProperty _layerProperty = null;

        private Shared<float> _dropDistance = new Shared<float>(10f);
        private FloatProperty _dropDistanceProperty = null;

        public DropModifier(ArrayCreator creator)
            : base(creator)
        {
            SetupProperties();

            _positions = new Vector3[Owner.CreatedObjects.Count];
            Drop();
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
            _layer.Set(_layerProperty.Update());
        }

        private void Drop()
        {
            List<GameObject> createdObjs = Owner.CreatedObjects;
            GameObject current = null;
            for (int i = 0; i < createdObjs.Count; ++i)
            {
                current = createdObjs[i];
                if (Physics.Raycast(current.transform.position, Vector3.down, out RaycastHit hit, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore))
                {
                    _positions[i] = hit.point;
                }
                else
                {
                    _positions[i] = current.transform.position;
                }
            }
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
            _layerProperty = new LayerProperty("LayerMask", _layer, OnLayerMaskChanged);
        }
    }
}
