using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public static class ModifierFactory
    {
        public delegate Modifier CreatorFucntion(ArrayCreator creator);
        private static Dictionary<string, CreatorFucntion> _creators = new Dictionary<string, CreatorFucntion>()
        {
            { ModifierType.ScaleRandom, (array) => { return new RandomScaleModifier(array); } },
            { ModifierType.ScaleUniform, (array) => { return new UniformScaleModifier(array); } },
            { ModifierType.RotationRandom, (array) => { return new RandomRotation(array); } },
            { ModifierType.RotationUniform, (array) => { return new UniformRotation(array); } },
            { ModifierType.FollowCurve, (array) => { return new FollowCurveModifier(array); } },
            { ModifierType.IncrementalRotation, (array) => { return new IncrementalRotationModifier(array); } }
        };

        public static Modifier CreateModifier(string modifierName, ArrayCreator array)
        {
            if (_creators.TryGetValue(modifierName, out CreatorFucntion func))
            {
                return func(array);
            }

            return null;
        }
    }
}