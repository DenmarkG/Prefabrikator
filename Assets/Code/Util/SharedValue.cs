using System;

namespace Prefabrikator
{
    [Serializable]
    public class Shared<T> where T : struct
    {
        [UnityEngine.SerializeField] private T _value = default(T);

        public event Action<T> OnValueChanged = null;

        public Shared(T t = default)
        {
            _value = t;
        }

        public Shared(T t, Action<T> onValueChaned)
        {
            _value = t;
            OnValueChanged = onValueChaned;
        }

        public static implicit operator T(Shared<T> t) => t._value;
        public static explicit operator Shared<T>(T t) => new Shared<T>(t);

        public void Set(T t)
        {
            _value = t;
            OnValueChanged?.Invoke(_value);
        }

        public T Get()
        {
            return _value;
        }

        public ref T GetRef()
        {
            return ref _value;
        }
    }

    [Serializable]
    public class SharedFloat : Shared<float> 
    {
        public SharedFloat(float n = default)
            : base(n) { }

        public SharedFloat(float n, Action<float> onValueChaned)
            : base(n, onValueChaned) { }
    }

    [Serializable]
    public class SharedInt : Shared<int> 
    {
        public SharedInt(int n = default)
            : base(n) { }

        public SharedInt(int n, Action<int> onValueChaned)
            : base(n, onValueChaned) { }
    }

    [Serializable]
    public class SharedVector3 : Shared<UnityEngine.Vector3> 
    {
        public SharedVector3(UnityEngine.Vector3 n = default)
            : base(n) { }

        public SharedVector3(UnityEngine.Vector3 n, Action<UnityEngine.Vector3> onValueChaned)
            : base(n, onValueChaned) { }
    }
}