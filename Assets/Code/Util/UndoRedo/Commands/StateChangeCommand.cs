using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    internal class StateChangeCommand : CreatorCommand
    {
        private ArrayState _previousState = null;
        private ArrayState _nextState = null;

        public StateChangeCommand(ArrayCreator creator, ArrayState currentState, ArrayState nextState)
            : base(creator)
        {
            _previousState = currentState;
            _nextState = nextState;
        }

        public override void Execute()
        {
            Creator.SetState(_nextState);
            Creator.Refresh();
        }

        public override void Revert()
        {
            Creator.SetState(_previousState);
            Creator.Refresh();
        }
    }
}
