using Prefabrikator.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public abstract class ShapeComponent : MonoBehaviour
    {
        // #DG: Need to add setting to toggle between single and multiple objects
        public GameObject Selection => _objectToClone;
        [SerializeField] private GameObject _objectToClone;

        public int Count => Collection?.Count ?? 0;
        public abstract List<Transform> Collection { get; }
        public abstract List<Modifier> Modifiers { get; }
        public abstract void OnRefresh();
        
        public void AddTransform(Transform xform)
        {
            Collection.Add(xform);
            Refresh();
        }

        private void OnValidate()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            OnRefresh();
        }

        public void SetSelection(GameObject selection)
        {
            _objectToClone = selection;
        }


#if UNITY_EDITOR
        //public delegate void ModifierFunction<T>(T shape) where T : IShape;
        public abstract Runtime.IShape GetShapeData();
#endif
    }
}