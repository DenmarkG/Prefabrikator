using System.Collections.Generic;
using UnityEngine;

namespace Prefabrikator
{
    public static class ModifierFactory
    {
        public delegate Modifier CreatorFucntion(ArrayCreator creator);
        private static Dictionary<string, CreatorFucntion> _creators = new Dictionary<string, CreatorFucntion>()
        {
            { ModifierType.ScaleRandom.ToString(), (array) => { return new RandomScaleModifier(array); } },
            { ModifierType.ScaleUniform.ToString(), (array) => { return new UniformScaleModifier(array); } },
            { ModifierType.RotationRandom.ToString(), (array) => { return new RandomRotation(array); } },
            { ModifierType.RotationUniform.ToString(), (array) => { return new UniformRotation(array); } },
            { ModifierType.FollowCurve.ToString(), (array) => { return new FollowCurveModifier(array); } },
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