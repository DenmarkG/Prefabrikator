using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

        protected SceneView _sceneView = null;
        protected EditMode _editMode = EditMode.None;

        public DropModifier(ArrayCreator creator)
            : base(creator)
        {
            SetupProperties();

            _positions = new List<Vector3>(Owner.CreatedObjects.Count);

            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void Teardown()
        {
            Owner.ApplyToAll((go, index) => { go.transform.position = Owner.GetDefaultPositionAtIndex(index); });
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Process(GameObject[] objs)
        {
            if (_positions == null)
            {
                _positions = new List<Vector3>(objs.Length);
            }

            if (_positions.Count < objs.Length)
            {
                while (_positions.Count < objs.Length)
                {
                    int index = objs.Length - (objs.Length - _positions.Count);
                    _positions.Add(objs[index].transform.position);
                }
            }
            else if (_positions.Count > objs.Length)
            {
                while (_positions.Count > objs.Length)
                {
                    _positions.RemoveAt(_positions.Count - 1);
                }
            }

            int numObjs = objs.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                objs[i].transform.position = _positions[i];
            }
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

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
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
                Collider collider = current.GetComponent<Collider>();

                RaycastHit[] hits = Physics.RaycastAll(start, Vector3.down, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != collider)
                    {
                        float offset = _verticalOffset;
                        if (_useCollider && collider != null)
                        {
                            offset = current.transform.InverseTransformPoint(collider.bounds.min).y;
                            Debug.Log($"Drop hieght = {offset}. Hit Y = {hit.point.y}");
                        }

                        _positions.Add(hit.point + (Vector3.down * offset));
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

        protected void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            if (_editMode.HasFlag(EditMode.Center))
            {
                Handles.color = Color.cyan;
                foreach (GameObject obj in Owner.CreatedObjects)
                {
                    Handles.DrawLine(obj.transform.position, obj.transform.position + (Vector3.up * _verticalOffset));
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
            _layerProperty = new LayerMaskProperty("LayerMask", _layer, OnLayerMaskChanged);

            void OnOffsetChanged(float current, float previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<float>(_verticalOffset, previous, current));
            }
            _offsetProperty = new FloatProperty("Vertical Offset", _verticalOffset, OnOffsetChanged);
            _offsetProperty.OnEditModeEnter += () => { _editMode = EditMode.Center; };
            _offsetProperty.OnEditModeExit += () => { _editMode &= ~EditMode.Center; };

            void OnUseColliderChanged(bool current, bool previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<bool>(_useCollider, previous, current));
            }
            _colliderProperty = new ToggleProperty("Use Collider for Offset", _useCollider, OnUseColliderChanged);
        }
    }
}
