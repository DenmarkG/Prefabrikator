using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public class RandomScaleModifier : Modifier
    {
        private Vector3[] _scales = null;
        private ArrayToolExtensions.MinMax _xRange = null;
        private ArrayToolExtensions.MinMax _yRange = null;
        private ArrayToolExtensions.MinMax _zRange = null;

        public override void Process(GameObject[] objs)
        {
            int numObjs = objs.Length;

            if (_scales == null || numObjs != _scales.Length)
            {
                _scales = new Vector3[numObjs];
                for (int i = 0; i < numObjs; i++)
                {
                    _scales[i] = ArrayToolExtensions.GetRandomUnitVector(_xRange, _yRange, _zRange);
                }
            }

            for (int i = 0; i < numObjs; ++i)
            {
                objs[i].transform.localScale = _scales[i];
            }
        }

        public override void UpdateInspector()
        {
            //
        }
    }
}
