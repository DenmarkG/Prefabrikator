using System;
using UnityEngine;

namespace Prefabrikator
{
    using Object = UnityEngine.Object;

    public interface ICommand
    {
        string Name { get; }
        void Execute(Object obj);
        void Revert(Object obj);
    }
}