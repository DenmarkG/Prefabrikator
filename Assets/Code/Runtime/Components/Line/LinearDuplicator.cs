using Prefabrikator.Shapes;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Runtime
{
    [ExecuteInEditMode]
    public class LinearDuplicator : LinearComponent
    {
        private static readonly int DefaultCount = 3;

        [SerializeField] private GameObject _original;

        public int Count
        {
            get => _count;
            set => _count.Set(value);
        }
        
        [SerializeField] private Shared<int> _count = new(DefaultCount);
        [SerializeField] private bool _keepOriginal;

        private Transform _transform;

        private void Awake()
        {
            _transform = this.transform;
            _count.OnValueChanged += OnCountChange;
        }

        private void OnDestroy()
        {
            _count.OnValueChanged -= OnCountChange;
        }

        public override void Refresh()
        {
            Line.Refresh(Objects, LineInternal);
        }

        public override void AddTransform(Transform xForm = null)
        {
            return;
        }

        private void Update()
        {
            //
        }

        private void OnCountChange(int _)
        {
            if (_count < Objects.Count)
            {
                while (Objects.Count > _count)
                {
                    int index = Objects.Count - 1;
                    if (index >= 0)
                    {
                        DestroyClone(Objects[Objects.Count - 1]);
                    }
                    else
                    {
                        break;
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

            }
        }

#if UNITY_EDITOR
        private GameObject _previousPrefab;
        private int _previousCount;

        private void OnEnable()
        {
            if (_original != _previousPrefab)
            {
                _previousPrefab = _original;
                OnPrefabChanged();
            }

            if (_previousCount != _count)
            {
                _previousCount = _count;
                OnCountChange(_count);
            }

            Refresh();
        }
#endif // UNITY_EDITOR
    }
}