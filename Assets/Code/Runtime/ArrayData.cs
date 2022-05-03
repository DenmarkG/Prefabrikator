using UnityEngine;

namespace Prefabrikator
{
    // #DG: Give this an Abstract factory

    [System.Serializable]
    public abstract class ArrayData
    {
        public ShapeType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        [SerializeField] private ShapeType _type;

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
        [SerializeField] private int _count = 0;

        public GameObject Prefab
        {
            get { return _prefab; }
            set { _prefab = value; }
        }
        [SerializeField] private GameObject _prefab = null;

        public Vector3 TargetScale
        {
            get { return _targetScale; }
            set { _targetScale = value; }
        }
        [SerializeField] private Vector3 _targetScale = Vector3.zero;

        public Quaternion TargetRotation
        {
            get { return _targetRotation; }
            set { _targetRotation = value; }
        }
        [SerializeField] private Quaternion _targetRotation = Quaternion.identity;

        public ArrayData(ShapeType type, GameObject prefab, Quaternion targetRotation)
        {
            _type = type;
            _prefab = prefab;
            _targetRotation = targetRotation;
        }
    }
}
