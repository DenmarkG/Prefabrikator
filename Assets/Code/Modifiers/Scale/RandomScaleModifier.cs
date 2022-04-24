using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class RandomScaleModifier : Modifier
    {
        protected override string DisplayName => "Random Scale";

        private Vector3[] _scales = null;
        private Extensions.MinMax _xRange = null;
        private Extensions.MinMax _yRange = null;
        private Extensions.MinMax _zRange = null;

        public RandomScaleModifier(ArrayCreator owner)
            : base(owner)
        {
            //
        }

        public override void Process(GameObject[] objs)
        {
            int numObjs = objs.Length;

            if (_scales == null || numObjs != _scales.Length)
            {
                _scales = new Vector3[numObjs];
                for (int i = 0; i < numObjs; i++)
                {
                    _scales[i] = Extensions.GetRandomUnitVector(_xRange, _yRange, _zRange);
                }
            }

            for (int i = 0; i < numObjs; ++i)
            {
                objs[i].transform.localScale = _scales[i];
            }
        }

        protected override void OnInspectorUpdate()
        {
            //
        }
    }
}
