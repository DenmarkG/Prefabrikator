using System;

public class Shared<T> where T : struct
{
    private T _value = default(T);

    public event Action<T> OnValueChanged = null;

    public Shared(T t = default(T))
    {
        _value = t;
    }

    public Shared(T t, Action<T> onValueChaned)
    {
        _value = t;
        OnValueChanged = onValueChaned;
    }

    public static implicit operator T(Shared<T> t) => t._value;

    public void Set(T t)
    {
        _value = t;
        OnValueChanged?.Invoke(_value);
    }

    public T Get()
    {
        return _value;
    }
}
