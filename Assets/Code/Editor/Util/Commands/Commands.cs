using UnityEngine;
using UnityEditor;

namespace Prefabrikator.Editor
{
    internal abstract class CreatorCommand : ICommand
    {
        protected IShape Creator => _creator;
        private IShape _creator = null;
        public abstract string Name { get; }

        public CreatorCommand(IShape creator)
        {
            _creator = creator;
        }

        public abstract void Execute(Object obj);
        public abstract void Revert(Object obj);
    }

    internal class GenericCommand<T> : ICommand where T : struct
    {
        private Shared<T> _watchedValue = new Shared<T>();
        private T _previousValue = default(T);
        private T _nextValue = default(T);
        public string Name => "Array value changed";

        public GenericCommand(Shared<T> watchedValue, T previous, T next)
        {
            _watchedValue = watchedValue;
            _previousValue = previous;
            _nextValue = next;
        }

        public void Execute(Object obj)
        {
            if (_watchedValue != null)
            {
                Undo.RecordObject(obj, Name);
                _watchedValue.Set(_nextValue);
            }
        }

        public void Revert(Object obj)
        {
            if (_watchedValue != null)
            {
                _watchedValue.Set(_previousValue);
            }
        }
    }

    internal abstract class ModifierCommand : ICommand
    {
        public string Name => "Modifier changed command";
        protected Modifier TargetModifier { get; private set; }

        public ModifierCommand(Modifier modifier)
        {
            TargetModifier = modifier;
        }

        public abstract void Execute(Object obj);
        public abstract void Revert(Object obj);
    }

    internal class CountChangeCommand : CreatorCommand
    {
        private int _previousCount;
        private int _nextCount;
        public override string Name => "Array count changed";


        public CountChangeCommand(IShape creator, int previousCount, int nextCount)
            : base(creator)
        {
            _previousCount = previousCount;
            _nextCount = nextCount;
        }

        public override void Execute(Object obj)
        {
            Creator.SetTargetCount(_nextCount);
            Creator.Refresh();
        }

        public override void Revert(Object obj)
        {
            Creator.SetTargetCount(_previousCount);
            Creator.Refresh();
        }
    }

    internal class ModifierAddCommand : ICommand
    {
        private Modifier _modifier = null;
        private IShape _creator = null;
        public string Name => "Add {0} modifier";

        public ModifierAddCommand(Modifier modifier, IShape creator)
        {
            _creator = creator;
            _modifier = modifier;
        }

        public void Execute(Object obj)
        {
            Undo.RecordObject(obj, string.Format(Name, _modifier.GetType().Name));
            _creator.AddModifier(_modifier);
        }

        public void Revert(Object obj)
        {
            _creator.RemoveModifier(_modifier);
        }
    }

    internal class ModifierRemoveCommand : ICommand
    {
        private Modifier _modifier = null;
        private IShape _creator = null;
        public string Name => "Modifier {0} removed";

        public ModifierRemoveCommand(Modifier modifier, IShape creator)
        {
            _creator = creator;
            _modifier = modifier;
        }

        public void Execute(Object obj)
        {
            Undo.RecordObject(obj, string.Format(Name, _modifier.GetType().Name));
            _creator.RemoveModifier(_modifier);
        }

        public void Revert(Object obj)
        {
            _creator.AddModifier(_modifier);
        }
    }

    public class ValueChangedCommand<T> : ICommand
    {
        private System.Action<T> OnValueChanged = null;

        private T _previous = default(T);
        private T _next = default(T);
        public string Name => "Array value change";

        public ValueChangedCommand(T previous, T next, System.Action<T> onValueChanged)
        {
            _previous = previous;
            _next = next;

            OnValueChanged = onValueChanged;
        }


        public void Execute(Object obj)
        {
            Undo.RecordObject(obj, Name);
            OnValueChanged(_next);
        }

        public void Revert(Object obj)
        {
            OnValueChanged(_previous);
        }
    }

    //#DG: add a command that can include multiple steps (eg. adding incresing a count also needs to store the new random value)
}
