using System;

public class Shared<T>
{
    public T Value => _value;
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

    // #DG: this is dangerous and may need a better solution
    // if T is ref type, can still be changed w/o callback
    public T Get()
    {
        return _value;
    }
}
