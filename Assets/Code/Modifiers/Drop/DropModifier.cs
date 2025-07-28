using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    // Add Physics simulation
    // https://stackoverflow.com/questions/58452586/how-to-let-gravity-work-in-editor-mode-in-unity3d
    class DropModifier : Modifier
    {
        private enum CollisionType
        {
            VisibleGeometry,
            CollisionGeometry
        }

        //[Serializable]
        //private class MeshWrapper
        //{
        //    public MeshFilter[] Meshes;
        //}

        public override string DisplayName => ModifierType.DropToFloor;

        [SerializeField] private Shared<LayerMask> _layer = new Shared<LayerMask>(LayerMask.NameToLayer("Default"));
        [SerializeField] private LayerMaskProperty _layerProperty = null;

        [SerializeField] private Shared<float> _dropDistance = new Shared<float>(10f);
        [SerializeField] private FloatProperty _dropDistanceProperty = null;

        [SerializeField] private Shared<bool> _useCollider = new Shared<bool>(true);
        [SerializeField] private ToggleProperty _colliderProperty = null;

        [SerializeField] private Shared<float> _verticalOffset = new Shared<float>();
        [SerializeField] private FloatProperty _offsetProperty = null;

        [SerializeField] protected SceneView _sceneView = null;
        [SerializeField] protected EditMode _editMode = EditMode.None;

        [SerializeField] private CollisionType _collisionType = CollisionType.CollisionGeometry;
        [SerializeField] private MeshFilter _targetMesh = null;

        // Generated Collision
        [SerializeField] private GameObject _dropTarget = null;

        [SerializeField] private bool _dropped = false;

        // #DG: remove this after separating editor code into separate class
        private IShape _target;

        public DropModifier(IShape target)
        {
            SetupProperties();
            this._target = target;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void Teardown(IShape target)
        {
            _target.ApplyToAll((_, go, index) => { go.transform.position = _target.GetDefaultPositionAtIndex(index); });
            SceneView.duringSceneGui -= OnSceneGUI;

            if (_dropTarget != null)
            {
                GameObject.DestroyImmediate(_dropTarget);
                _dropTarget = null;
            }
        }

        public override void OnRemoved(IShape target)
        {
            Teardown(target);
        }

        public override TransformProxy[] Process(IShape target, TransformProxy[] proxies)
        {
            if (_dropped)
            {
                if (_collisionType == CollisionType.VisibleGeometry)
                {
                    if (_targetMesh != null)
                    {
                        GenerateCollision();
                    }
                }

                TransformProxy current;
                int numObjs = proxies.Length;
                for (int i = 0; i < numObjs; ++i)
                {
                    current = proxies[i];
                    Vector3 start = current.Position;
                    Collider collider = _target.Clones[i].GetComponent<Collider>();

                    float offset = _verticalOffset;
                    if (collider != null)
                    {
                        if (_useCollider)
                        {
                            offset = _target.Clones[i].transform.InverseTransformPoint(collider.bounds.min).y;
                            start -= Vector3.down * offset;
                        }

                        collider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }

                    if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore))
                    {
                        Debug.DrawLine(start, hit.point, Color.red);
                        proxies[i].Position = hit.point; // + (Vector3.down * offset);
                    }
                    else
                    {
                        Debug.DrawLine(start, hit.point, Color.white);
                    }
                }
            }
            return proxies;
        }

        protected override void OnInspectorUpdate(IShape target)
        {
            _dropDistance.Set(_dropDistanceProperty.Update());
            _useCollider.Set(_colliderProperty.Update());

            if (_useCollider == false)
            {
                _verticalOffset.Set(_offsetProperty.Update());
            }

            CollisionType collisionType = (CollisionType)EditorGUILayout.EnumPopup("Detection Type", _collisionType);
            if (collisionType != _collisionType)
            {
                _target.CommandQueue.Enqueue(new ValueChangedCommand<CollisionType>(_collisionType, collisionType, x => { _collisionType = x; }));
            }

            if (_collisionType == CollisionType.VisibleGeometry)
            {
                MeshFilter targetMesh = EditorGUILayout.ObjectField("Mesh", _targetMesh, typeof(MeshFilter), true) as MeshFilter;
                if (targetMesh != _targetMesh)
                {
                    _target.CommandQueue.Enqueue(new ValueChangedCommand<MeshFilter>(_targetMesh, targetMesh, x => { _targetMesh = x; }));
                }
            }

            _layer.Set(_layerProperty.Update());

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Drop"))
                {
                    Drop();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }

        private void Drop()
        {
            _dropped = true;
        }

        private void Apply(Vector3[] positions)
        {
            _dropped = !_dropped;
            _target.ApplyToAll((_, go, index) => { go.transform.position = positions[index]; });
        }

        private void GenerateCollision()
        {
            if (_dropTarget == null)
            {
                _dropTarget = new GameObject("Drop Target");
                _dropTarget.transform.SetPositionAndRotation(_targetMesh.transform.position, _targetMesh.transform.rotation);
                _dropTarget.transform.localScale = _targetMesh.transform.localScale;

                // #DG: clear the drop target when the mesh target changes in the inspector
                if (_targetMesh != null)
                {
                    // #DG: Unity bug won't allow this to work in editor. Bring back when fixed
                    MeshCollider collider = _dropTarget.AddComponent<MeshCollider>();

                    Mesh mesh = new()
                    {
                        vertices = _targetMesh.sharedMesh.vertices,
                        triangles = _targetMesh.sharedMesh.triangles,
                        normals = _targetMesh.sharedMesh.normals,
                        tangents = _targetMesh.sharedMesh.tangents
                    };

                    Physics.BakeMesh(mesh.GetInstanceID(), true);
                    Physics.SyncTransforms();

                    //collider.sharedMesh = _targetMesh.sharedMesh;
                    collider.sharedMesh = mesh;
                    //collider.convex = true;
                }
            }
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
                foreach (Transform obj in _target.Clones)
                {
                    Handles.DrawLine(obj.transform.position, obj.transform.position + (Vector3.up * _verticalOffset));
                }
            }

            if (_dropped)
            {
                int numObjs = _target.Clones.Count;
                for (int i = 0; i < numObjs; ++i)
                {
                    Transform current = _target.Clones[i];
                    Vector3 start = current.transform.position;
                    Collider collider = current.GetComponent<Collider>();
                    float offset = _verticalOffset;
                    if (_useCollider && collider != null)
                    {
                        offset = current.transform.InverseTransformPoint(collider.bounds.min).y;
                        start -= Vector3.down * offset;
                    }

                    Color defaultColor = Handles.color;
                    if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore))
                    {
                        Handles.color = Color.red;
                        Handles.DrawLine(start, hit.point);
                    }
                    else
                    {
                        Handles.color = Color.white;
                        Debug.DrawLine(start, start + (Vector3.down * _dropDistance), Color.white);
                    }

                    Handles.color = defaultColor;
                }
            }
        }

        private void SetupProperties()
        {
            void OnDropDistanceChanged(float current, float previous)
            {
                _target.CommandQueue.Enqueue(new GenericCommand<float>(_dropDistance, previous, current));
            }
            _dropDistanceProperty = new FloatProperty("Max Drop Distance", _dropDistance, OnDropDistanceChanged);

            void OnLayerMaskChanged(LayerMask current, LayerMask previous)
            {
                _target.CommandQueue.Enqueue(new GenericCommand<LayerMask>(_layer, previous, current));
            }
            _layerProperty = new LayerMaskProperty("LayerMask", _layer, OnLayerMaskChanged);

            void OnOffsetChanged(float current, float previous)
            {
                _target.CommandQueue.Enqueue(new GenericCommand<float>(_verticalOffset, previous, current));
            }
            _offsetProperty = new FloatProperty("Vertical Offset", _verticalOffset, OnOffsetChanged);
            _offsetProperty.OnEditModeEnter += () => { _editMode = EditMode.Center; };
            _offsetProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Center; };

            void OnUseColliderChanged(bool current, bool previous)
            {
                _target.CommandQueue.Enqueue(new GenericCommand<bool>(_useCollider, previous, current));
            }

            const string tooltip = "Set this to TRUE to use the bottom of the collider bounds to detect as the drop point";
            _colliderProperty = new ToggleProperty(new GUIContent("Use Collider for Offset", tooltip), _useCollider, OnUseColliderChanged);
        }
    }
}
