using Codice.Client.Common.GameUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    // #DG: TODO: intercept object destruction and remove from values
    // or 
    // remove any null refs during update step

    internal enum TargetObject
    {
        //This, // #DG: causes stack overflow. Need to remove all arrays before duplicating
        Prefab,
        Children
    }

    [ExecuteInEditMode]
    public class LineArray : MonoBehaviour
    {
        private static readonly int MaxCount = 50;
        private static readonly int MinCount = 0;

        [SerializeField] private TargetObject _objectToClone = TargetObject.Prefab;

#if UNITY_EDITOR
        private TargetObject _previousObjectToClone = TargetObject.Prefab;
#endif // UNITY_EDITOR

        [SerializeField] private Shared<Vector3> _offset = new(new Vector3(2f, 0f, 0f));
        [SerializeField] private Shared<Vector3> _start = new(); // #DG: make relative to this gameobject position
        [SerializeField] private Shared<int> _count = new(); // #DG: Hide when target is children
        [SerializeField] private GameObject _prefab = null;

        private List<GameObject> _clones = null;
        private Transform _transform = null;

        private void Awake()
        {
            _transform = this.transform;
        }

        private void Update()
        {
            _clones ??= new List<GameObject>(_count);
            _transform ??= this.transform; // #DG: edit time only

            if (_count != _clones.Count)
            {
                if (_count > _clones.Count)
                {
                    while (_count > _clones.Count)
                    {
                        GameObject clone = null;

                        if (_objectToClone == TargetObject.Prefab)
                        {
                            clone = GameObject.Instantiate(_prefab, this.transform);
                        }
                        else
                        {
                            int index = _clones.Count;
                            if (index > 0 && index < _count)
                            {
                                clone = this.transform.GetChild(index).gameObject;
                            }
                        }

                        clone.transform.position = _start + (_offset.Get() * _clones.Count);
                        _clones.Add(clone);
                    }

                }
                else if (_count < _clones.Count)
                {
                    while (_count < _clones.Count)
                    {
                        int index = _clones.Count - 1;
                        GameObject clone = _clones[index];

                        if (clone != null)
                        {
                            if (Application.isPlaying)
                            {
                                GameObject.Destroy(clone);
                            }
                            else
                            {
                                GameObject.DestroyImmediate(clone);
                            }
                        }

                        _clones.RemoveAt(index);
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (_objectToClone != _previousObjectToClone)
            {
                if (_objectToClone == TargetObject.Children)
                {
                    _count.Set(this.transform.childCount);
                }
            }

            int numObjects = _clones?.Count ?? 0;
            GameObject current = null;
            for (int i = 0; i < numObjects; ++i)
            {
                if ((current = _clones[i]) != null)
                {
                    current.transform.position = _start + (_offset.Get() * i);
                }
            }

            _count.Set(Mathf.Clamp(_count, MinCount, MaxCount));
        }

        public void Refresh()
        {
            //
        }

#if UNITY_EDITOR
        public void InitFromSharedData(Shared<Vector3> start, Shared<Vector3> offset, Shared<int> count)
        {
            _start = start;
            _offset = offset;
            _count = count;
        }
#endif // UNITY_EDITOR
    }
}
