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
        private Vector3 _targetScale = new Vector3(1, 1, 1);
        private Vector3Property _displayField = null;

        public UniformScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            _displayField = new Vector3Property("Scale", new Vector3(1, 1, 1), OnValueChanged);
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
            _targetScale = _displayField.Update();
        }

        public void OnValueChanged(Vector3 current, Vector3 previous)
        {
            Owner.CommandQueue.Enqueue(new OnUniformScaleChangeCommand(this, previous, current));
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

                    scaleMod._displayField.SetDefaultValue(NextScale);
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

                    scaleMod._displayField.SetDefaultValue(PreviousScale);
                }
            }
        }
    }
}
