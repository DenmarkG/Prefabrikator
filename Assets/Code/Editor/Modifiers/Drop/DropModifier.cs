using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
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

        private static readonly int MaxBufferSize = 32;

        //[Serializable]
        //private class MeshWrapper
        //{
        //    public MeshFilter[] Meshes;
        //}

        protected override string DisplayName => ModifierType.DropToFloor;

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

        private CollisionType _collisionType = CollisionType.CollisionGeometry;
        private MeshFilter _targetMesh = null;

        // Generated Collision
        private GameObject _dropTarget = null;

        private bool _dropped = false;

        public DropModifier(ArrayCreator creator)
            : base(creator)
        {
            SetupProperties();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void Teardown()
        {
            Owner.ApplyToAll((go, index) => { go.transform.position = Owner.GetDefaultPositionAtIndex(index); });
            SceneView.duringSceneGui -= OnSceneGUI;

            if (_dropTarget != null)
            {
                GameObject.DestroyImmediate(_dropTarget);
                _dropTarget = null;
                EditorSceneManager.UnloadSceneAsync(_scene);
            }
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Process(GameObject[] objs)
        {
            if (_dropped == true)
            {
                Vector3[] positions = new Vector3[objs.Length];
                foreach (GameObject go in objs)
                {
                    if (_collisionType == CollisionType.VisibleGeometry)
                    {
                        if (_targetMesh != null)
                        {
                            GenerateCollision();
                        }
                    }

                    GameObject current = null;
                    for (int i = 0; i < objs.Length; ++i)
                    {
                        current = objs[i];
                        Vector3 start = current.transform.position;
                        Collider collider = current.GetComponent<Collider>();

                        RaycastHit[] hits;
                        if (_targetMesh != null)
                        {
                            hits = new RaycastHit[MaxBufferSize];
                            _physicsScene.Raycast(start, Vector3.down, hits, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore);
                        }
                        else
                        {
                            hits = Physics.RaycastAll(start, Vector3.down, _dropDistance, ~_layer.Get(), QueryTriggerInteraction.Ignore);
                        }

                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.collider != collider)
                            {
                                float offset = _verticalOffset;
                                if (_useCollider && collider != null)
                                {
                                    offset = current.transform.InverseTransformPoint(collider.bounds.min).y;
                                }

                                positions[i] = hit.point + (Vector3.down * offset);
                                break;
                            }
                        }
                    }
                }

                int numObjs = objs.Length;
                for (int i = 0; i < numObjs; ++i)
                {
                    objs[i].transform.position = positions[i];
                }
            }
            else
            {
                int numObjs = objs.Length;
                for (int i = 0; i < numObjs; ++i)
                {
                    objs[i].transform.position = Owner.GetDefaultPositionAtIndex(i);
                }
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

            CollisionType collisionType = (CollisionType)EditorGUILayout.EnumPopup("Detection Type", _collisionType);
            if (collisionType != _collisionType)
            {
                Owner.CommandQueue.Enqueue(new ValueChangedCommand<CollisionType>(_collisionType, collisionType, x => { _collisionType = x; }));
            }

            if (_collisionType == CollisionType.VisibleGeometry)
            {
                MeshFilter targetMesh = EditorGUILayout.ObjectField("Mesh", _targetMesh, typeof(MeshFilter), true) as MeshFilter;
                if (targetMesh != _targetMesh)
                {
                    Owner.CommandQueue.Enqueue(new ValueChangedCommand<MeshFilter>(_targetMesh, targetMesh, x => { _targetMesh = x; }));
                }
            }

            _layer.Set(_layerProperty.Update());

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Drop"))
                {
                    Drop();
                }
                else if (GUILayout.Button("Reset"))
                {
                    Reset();
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

        private void Reset()
        {
            _dropped = false;
        }

        private Scene _scene;
        private PhysicsScene _physicsScene;

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

                    CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);

                    _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    _scene.name = "Prefabrikator";
                    SceneManager.MoveGameObjectToScene(_dropTarget, _scene);

                    _physicsScene = _scene.GetPhysicsScene();
                    Physics.autoSimulation = false;


                    MeshCollider collider = _dropTarget.AddComponent<MeshCollider>();

                    Mesh mesh = new()
                    {
                        vertices = _targetMesh.sharedMesh.vertices,
                        triangles = _targetMesh.sharedMesh.triangles,
                        normals = _targetMesh.sharedMesh.normals,
                        tangents = _targetMesh.sharedMesh.tangents
                    };

                    Debug.Log($"mesh is readable = {mesh.isReadable}");
                    Physics.BakeMesh(mesh.GetInstanceID(), true);

                    //collider.sharedMesh = _targetMesh.sharedMesh;
                    collider.sharedMesh = mesh;
                    collider.convex = true;

                    _physicsScene.Simulate(Time.fixedDeltaTime * 10);
                    //Physics.Simulate(Time.fixedDeltaTime);
                    Physics.autoSimulation = true;
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
            _dropDistanceProperty = new FloatProperty("Max Drop Distance", _dropDistance, OnDropDistanceChanged);

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
            _offsetProperty.OnEditModeExit += (_) => { _editMode &= ~EditMode.Center; };

            void OnUseColliderChanged(bool current, bool previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<bool>(_useCollider, previous, current));
            }

            const string tooltip = "Set this to TRUE to use the bottom of the collider bounds to detect as the drop point";
            _colliderProperty = new ToggleProperty(new GUIContent("Use Collider for Offset", tooltip), _useCollider, OnUseColliderChanged);
        }
    }
}
