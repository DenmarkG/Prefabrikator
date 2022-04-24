using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public abstract class Modifier
    {
        protected Queue<ICommand> CommandQueue => _commandQueue;
        private Queue<ICommand> _commandQueue = new Queue<ICommand>();

        public abstract void UpdateInspector();
        public abstract void Process(GameObject[] objs);
    }
}