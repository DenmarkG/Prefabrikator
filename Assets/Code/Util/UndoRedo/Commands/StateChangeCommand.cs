using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    internal class StateChangeCommand : CreatorCommand
    {
        private ArrayData _previousState = null;
        private ArrayData _nextState = null;

        public StateChangeCommand(ArrayCreator creator, ArrayData currentState, ArrayData nextState)
            : base(creator)
        {
            _previousState = currentState;
            _nextState = nextState;
        }

        public override void Execute()
        {
            Creator.SetStateData(_nextState);
            Creator.Refresh();
        }

        public override void Revert()
        {
            Creator.SetStateData(_previousState);
            Creator.Refresh();
        }
    }
}
