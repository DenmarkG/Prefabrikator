using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Prefabrikator
{
    public class ScatterBoxCreator : ScatterVolumeCreator
    {
        private static readonly Vector3 DefaultSize = new Vector3(5f, 2f, 5f);
        private Bounds _bounds;

        private SceneView _sceneView = null;

        public ScatterBoxCreator(GameObject target)
            : base(target)
        {
            _bounds = new Bounds(target.transform.position, DefaultSize);

            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override string Name => "Scatter Box";

        ~ScatterBoxCreator()
        {
            Teardown();
        }

        protected override void OnSave()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public override void Teardown()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
            base.Teardown();
        }

        protected override void CreateClone(int index = 0)
        {
            GameObject proxy = GetProxy();

            if (proxy != null)
            {
                Vector3 position = GetRandomPointInBounds();
                GameObject clone = GameObject.Instantiate(_target, position, _target.transform.rotation);
                clone.SetActive(true);
                clone.transform.SetParent(proxy.transform);

                Positions.Add(position);
                _createdObjects.Add(clone);
            }
        }

        protected override ArrayData GetContainerData()
        {
            // #DG: TODO
            return null;
        }

        protected override Vector3 GetRandomPointInBounds()
        {
            return _bounds.GetRandomPointInBounds();
        }

        protected override void PopulateFromExistingData(ArrayData data)
        {
            // #DG: TODO
        }

        protected override void Scatter()
        {
            Positions.Clear();

            int count = _createdObjects.Count;

            for (int i = 0; i < count; ++i)
            {
                Vector3 position = GetRandomPointInBounds();
                _createdObjects[i].transform.position = position;
                Positions.Add(position);
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (_sceneView == null || _sceneView != view)
            {
                _sceneView = view;
            }

            Handles.DrawWireCube(_bounds.center, _bounds.size);
        }

        protected override void DrawVolumeEditor()
        {
            Vector3 center = EditorGUILayout.Vector3Field("Center", _bounds.center);
            Vector3 size = EditorGUILayout.Vector3Field("Size", _bounds.size);

            _bounds.center = center;
            _bounds.size = size;

            if (_sceneView != null)
            {
                EditorUtility.SetDirty(_sceneView);
            }
        }
    }
}
