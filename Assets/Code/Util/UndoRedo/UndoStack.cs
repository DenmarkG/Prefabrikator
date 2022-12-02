using System;
using System.Collections.Generic;

namespace Prefabrikator
{
    class UndoStack
    {
        public int UndoOperationsAvailable => _undoStack.Count;
        private Stack<ICommand> _undoStack = null;

        public int RedoOperationsAvailable => _redoStack.Count;
        private Stack<ICommand> _redoStack = null;

        public UndoStack(int maxStackSize = 25)
        {
            _undoStack = new Stack<ICommand>(maxStackSize);
            _redoStack = new Stack<ICommand>(maxStackSize);
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                ICommand command = _undoStack.Pop();
                command.Revert();

                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                ICommand command = _redoStack.Pop();
                command.Execute();

                _undoStack.Push(command);
            }
        }

        public void OnCommandExecuted(ICommand command)
        {
            if (_redoStack.Count > 0)
            {
                _redoStack.Clear();
            }

            _undoStack.Push(command);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
