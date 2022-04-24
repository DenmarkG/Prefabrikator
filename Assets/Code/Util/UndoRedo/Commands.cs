using UnityEngine;

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
}
