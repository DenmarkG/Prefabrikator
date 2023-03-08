using UnityEngine;

namespace Prefabrikator
{
    public class RadialNoise : RandomModifier
    {
        protected override string DisplayName => ModifierType.RadialNoise;

        private IRadial _radialShape = null;
        private float[] _radii = null;
        

        public RadialNoise(ArrayCreator creator)
            : base(creator) 
        {
            _radialShape = creator as IRadial;
            Debug.Assert(_radialShape != null, "Not a radial Shape. Cannot create radial noise");
            Randomize();
        }

        public override void OnRemoved()
        {
            Teardown();
        }

        public override void Process(GameObject[] objs)
        {
            //
        }

        public override void Teardown()
        {
            Owner.ApplyToAll((go, index) =>
            {
                go.transform.position = Owner.GetDefaultPositionAtIndex(index);
            });
        }

        protected override void OnInspectorUpdate()
        {
            if (GUILayout.Button("Randomize"))
            {
                Randomize();
            }
        }

        protected override void Randomize(int startingIndex = 0)
        {
            //
        }

        private void SetupProperties()
        {
            const string Min = "Min";
            const string Max = "Max";

            void OnMinChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_min, previous, current));
            }
            _minProperty = new Vector3Property(Min, _min, OnMinChanged);

            void OnMaxChanged(Vector3 current, Vector3 previous)
            {
                Owner.CommandQueue.Enqueue(new GenericCommand<Vector3>(_max, previous, current));
            }
            _maxProperty = new Vector3Property(Max, _max, OnMaxChanged);
        }
    }
}
