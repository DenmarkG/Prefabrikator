using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class UniformScaleModifier : Modifier
    {
        protected override string DisplayName => "Uniform Scale";

        public GameObject[] Targets => _targets;
        private GameObject[] _targets = null;
        private Shared<Vector3> _targetScale = new Shared<Vector3>(new Vector3(1f, 1f, 1f));
        private Vector3Property _targetScaleProperty = null;

        public UniformScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            _targetScaleProperty = new Vector3Property("Scale", _targetScale, OnValueChanged);
        }

        public override void Process(GameObject[] objs)
        {
            if (_targets == null || objs.Length != _targets.Length)
            {
                _targets = objs;
            }

            int numObjs = objs.Length;
            for (int i = 0; i < numObjs; ++i)
            {
                objs[i].transform.localScale = _targetScale;
            }
        }

        protected override void OnInspectorUpdate()
        {
            _targetScaleProperty.Update();
        }

        public void OnValueChanged(Vector3 current, Vector3 previous)
        {
            Owner.CommandQueue.Enqueue(new OnUniformScaleChangeCommand(this, previous, current));
        }

        public override void OnRemoved()
        {
            Vector3 defaultScale = Owner.GetDefaultScale();
            Owner.ApplyToAll((go) => { go.transform.localScale = defaultScale; });
        }

        private class OnUniformScaleChangeCommand : ModifierCommand
        {
            public Vector3 PreviousScale { get; set; }
            public Vector3 NextScale { get; set; }

            public OnUniformScaleChangeCommand(Modifier modifier, Vector3 previousScale, Vector3 nextScale)
                : base(modifier)
            {
                PreviousScale = previousScale;
                NextScale = nextScale;
            }

            public override void Execute()
            {
                if (TargetModifier is UniformScaleModifier scaleMod)
                {
                    if (scaleMod.Targets != null)
                    {
                        GameObject[] targets = scaleMod.Targets;
                        int count = targets.Length;
                        for (int i = 0; i < count; ++i)
                        {
                            targets[i].transform.localScale = NextScale;
                        }
                    }

                    scaleMod._targetScaleProperty.SetDefaultValue(NextScale);
                }
            }

            public override void Revert()
            {
                if (TargetModifier is UniformScaleModifier scaleMod)
                {
                    if (scaleMod.Targets != null)
                    {
                        GameObject[] targets = scaleMod.Targets;
                        int count = targets.Length;
                        for (int i = 0; i < count; ++i)
                        {
                            targets[i].transform.localScale = PreviousScale;
                        }
                    }

                    scaleMod._targetScaleProperty.SetDefaultValue(PreviousScale);
                }
            }
        }
    }
}
