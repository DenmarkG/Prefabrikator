using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator.Shapes
{
    public interface IShapeData { }

    public abstract class Shape<T> where T : IShapeData 
    {
        // #DG: may want to include shape here: T shape { get; private set; }
        public abstract Vector3 GetDefaultPositionAtIndex(int index, T data);
    }
}