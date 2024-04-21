using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShapeData { }
    public abstract class Shape<T> where T : IShapeData 
    {
        public abstract Vector3 GetDefaultPositionAtIndex(int index, T data);
    }
}