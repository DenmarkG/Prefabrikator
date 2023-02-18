using UnityEngine;
using System.Collections.Generic;

namespace Prefabrikator
{
    internal abstract class CreatorCommand : ICommand
    {
        protected ArrayCreator Creator => _creator;
        private ArrayCreator _creator = null;

        public CreatorCommand(ArrayCreator creator)
        {
            _creator = creator;
        }

        public abstract void Execute();
        public abstract void Revert();
    }

    internal class GenericCommand<T> : ICommand where T : struct
    {
        private Shared<T> _watchedValue = new Shared<T>();
        private T _previousValue = default(T);
        private T _nextValue = default(T);

        public GenericCommand(Shared<T> watchedValue, T previous, T next)
        {
            _watchedValue = watchedValue;
            _previousValue = previous;
            _nextValue = next;
        }

        public void Execute()
        {
            if (_watchedValue != null)
            {
                _watchedValue.Set(_nextValue);
            }
        }

        public void Revert()
        {
            if (_watchedValue != null)
            {
                _watchedValue.Set(_previousValue);
            }
        }
    }

    internal abstract class ModifierCommand : ICommand
    {
        protected Modifier TargetModifier { get; private set; }   

        public ModifierCommand(Modifier modifier)
        {
            TargetModifier = modifier;
        }

        public abstract void Execute();
        public abstract void Revert();
    }

    internal class CountChangeCommand : CreatorCommand
    {
        private int _previousCount;
        private int _nextCount;

        ArrayState _previousState = null;
        ArrayState _nextState = null;

        public CountChangeCommand(ArrayCreator creator, int previousCount, int nextCount)
            : base(creator)
        {
            _previousCount = previousCount;
            _nextCount = nextCount;
        }

        public override void Execute()
        {
            Creator.SetTargetCount(_nextCount);
            Creator.Refresh();
        }

        public override void Revert()
        {
            Creator.SetTargetCount(_previousCount);
            Creator.Refresh();
        }
    }

    internal class ModifierAddCommand : ICommand
    {
        private Modifier _modifier = null;
        private ArrayCreator _creator = null;

        public ModifierAddCommand(Modifier modifier, ArrayCreator creator)
        {
            _creator = creator;
            _modifier = modifier;
        }

        public void Execute()
        {
            _creator.AddModifier(_modifier);
            _creator.ProcessModifiers();
        }

        public void Revert()
        {
            _creator.RemoveModifier(_modifier);
            _creator.ProcessModifiers();
        }
    }

    internal class ModifierRemoveCommand : ICommand
    {
        private Modifier _modifier = null;
        private ArrayCreator _creator = null;

        public ModifierRemoveCommand(Modifier modifier, ArrayCreator creator)
        {
            _creator = creator;
            _modifier = modifier;
        }

        public void Execute()
        {
            _creator.RemoveModifier(_modifier);
            _creator.ProcessModifiers();
        }

        public void Revert()
        {
            _creator.AddModifier(_modifier);
            _creator.ProcessModifiers();
        }
    }

    internal class ValueChangedCommand<T> : ICommand
    {
        private T _previous = default(T);
        private T _next = default(T);

        private System.Action<T> OnValueChanged = null;

        public ValueChangedCommand(T previous, T next, System.Action<T> onValueChanged)
        {
            _previous = previous;
            _next = next;

            OnValueChanged = onValueChanged;
        }

        public void Execute()
        {
            OnValueChanged(_next);
        }

        public void Revert()
        {
            OnValueChanged(_previous);
        }
    }

    //#DG: add a command that can include multiple steps (eg. adding incresing a count also needs to store the new random value)
}
