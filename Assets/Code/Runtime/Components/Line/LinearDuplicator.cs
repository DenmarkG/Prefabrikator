using Prefabrikator.Shapes;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [ExecuteInEditMode]
    public class LinearDuplicator : LinearComponent
    {
        private static readonly int DefaultCount = 3;

        [SerializeField][HideInInspector] private List<Transform> _objects = new();
        [SerializeField] private GameObject _original;

        public int Count => _count;
        [SerializeField] private int _count = DefaultCount;
        [SerializeField] private bool _keepOriginal;

        private Transform _transform;

        private bool _isDirty = false;

        private void Awake()
        {
            _transform = this.transform;
        }

        private void Update()
        {
            if (_isDirty)
            {
                Refresh();
            }
        }

        public override void Refresh()
        {
            OnCountChange();
            LineInternal.Refresh();
        }

        public void SetCount(int count)
        {
            _count = Mathf.Max(count, 0);
            OnCountChange();
        }

        private void OnCountChange()
        {
            if (_original == null)
                return;

            if (_count < Objects.Count)
            {
                while (Objects.Count > _count)
                {
                    int index = Objects.Count - 1;
                    if (index >= 0)
                    {
                        var clone = Objects[index];
                        Objects.RemoveAt(index);
                        DestroyClone(clone);
                    }
                }
            }
            else
            {
                while (_count > Objects.Count)
                {
                    CreateClone();
                }
            }
        }

        private void DestroyClone(Transform clone)
        {
            if (Application.isPlaying)
            {
                Destroy(clone.gameObject);
            }
            else
            {
                DestroyImmediate(clone.gameObject);
            }
        }

        private void CreateClone()
        {
            if (_original != null)
            {
                GameObject clone = GameObject.Instantiate(_original, _original.transform.position, _original.transform.rotation, _original.transform.parent);
                clone.SetActive(true);
                clone.transform.SetParent(_transform);

                int lastIndex = Objects.Count - 1;

                if (Objects.Count > 0)
                {
                    clone.transform.position = Objects[lastIndex].transform.position + Offset;
                    clone.transform.rotation = Objects[lastIndex].transform.rotation;
                }
                else
                {
                    clone.transform.position = _transform.position + Offset;
                    clone.transform.rotation = _original.transform.rotation;
                }

                Objects.Add(clone.transform);
            }
        }

        private void OnPrefabChanged()
        {
            if (Objects.Count > 0)
            {
                for (int i = Objects.Count; i >= 0; --i)
                {
                    Transform obj = Objects[i];
                    DestroyClone(obj);
                }

                Objects.Clear();
            }

            if (_original != null)
            {
                if (_count > 0)
                {
                    OnCountChange();
                    Refresh();
                }
            }
        }

#if UNITY_EDITOR
        private GameObject _previousPrefab;

        [ContextMenu("Refresh")]
        private void EditorRefresh()
        {
            Refresh();
        }

        private void OnValidate()
        {
            if (_original != null)
            {
                if (_original != _previousPrefab)
                {
                    _previousPrefab = _original;
                    OnPrefabChanged(); // #DG: this won't work b/c of the destroy issue
                    _isDirty = true;
                }
            }
            
            _count = Mathf.Max(_count, 0);
            if (_count != Objects.Count)
            {
                _isDirty = true;
            }
        }

#endif // UNITY_EDITOR
    }
}