using System;

namespace Prefabrikator
{
    public interface ICommand
    {
        void Execute();
        void Revert();
    }
}